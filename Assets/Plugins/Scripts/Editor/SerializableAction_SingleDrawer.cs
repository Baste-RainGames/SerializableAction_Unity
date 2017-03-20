using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    [CustomPropertyDrawer(typeof(SerializableAction_Single))]
    public class SerializableAction_SingleDrawer : PropertyDrawer
    {
        private static Dictionary<Type, List<SerializableMethod>> typeToMethod = new Dictionary<Type, List<SerializableMethod>>();
        private static Dictionary<Type, string[]> typeToMethodNames = new Dictionary<Type, string[]>();
        private const string NoMethodSelected = "None";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return FindSerializableActionHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label);
            EditorGUI.indentLevel++;
            position = NextPosition(position, EditorGUIUtility.singleLineHeight);
            DrawSerializableAction(position, property);
            EditorGUI.indentLevel--;
        }

        public static float FindSerializableActionHeight(SerializedProperty property, GUIContent label)
        {
            float defaultHeight = EditorGUIUtility.singleLineHeight + 1;
            var action = (SerializableAction_Single) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

            if (action.TargetObject == null)
                return defaultHeight * 2; //label + object field

            //label + object field + method field. 1 less if there's no label
            var propHeightWithMethod = defaultHeight * (label == GUIContent.none ? 2 : 3);
            if (action.TargetMethod == null || action.TargetMethod.MethodInfo == null)
                return propHeightWithMethod;

            var parameterCount = action.TargetMethod.ParameterTypes.Length;
            if (parameterCount == 0)
                return propHeightWithMethod;

            var finalPropHeight = propHeightWithMethod;
            for (var i = 0; i < action.TargetMethod.ParameterTypes.Length; i++)
            {
                var parameterType = action.TargetMethod.ParameterTypes[i];
                finalPropHeight += SerializableArgumentDrawer.GetHeightForType(parameterType.SystemType, defaultHeight);
            }

            return finalPropHeight;
        }

        /*
         * This OnGUI mostly uses the raw object instead of fiddling with the SerializedProperty.
         * This is because we need to handle things like raw System.Objects and arrays,
         * both of which SerializedProperties can't handle properly.
         *
         * static so the list drawer can use it
         */
        public static void DrawSerializableAction(Rect position, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            var action = (SerializableAction_Single) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

            Object containingObject = action.TargetObject;
            if (action.TargetObject != null && action.TargetObject is Component)
                containingObject = (action.TargetObject as Component).gameObject;

            var callStateRect = position;
            callStateRect.width *= .3f;
            action.callState = (UnityEventCallState) EditorGUI.EnumPopup(callStateRect, action.callState);

            var objectRect = NextPosition(callStateRect, EditorGUIUtility.singleLineHeight);
            var newContainingObject = EditorGUI.ObjectField(objectRect, containingObject, typeof(Object), true);

            if (newContainingObject != containingObject)
                action.TargetObject = newContainingObject;
            containingObject = newContainingObject;

            if (action.TargetObject == null)
            {
                if (EditorGUI.EndChangeCheck())
                    property.SetDirty();
                return;
            }

            int objectIdx;
            Object[] objectsOnContainer;
            string[] objectNames;
            ReadyObjectSelectDropdown(containingObject, action.TargetObject, out objectsOnContainer, out objectNames, out objectIdx);

            var objectSelectRect = position;
            objectSelectRect.xMin = callStateRect.xMax + 5f;

            var objectSelectRect_label = objectSelectRect;
            var objectSelectRect_popup = objectSelectRect;
            objectSelectRect_label.width = 90f;
            objectSelectRect_popup.xMin = objectSelectRect_label.xMax + 5f;

            EditorGUI.LabelField(objectSelectRect_label, "Target Script:");
            var newIdx = EditorGUI.Popup(objectSelectRect_popup, objectIdx, objectNames);
            if (newIdx != objectIdx)
            {
                objectIdx = newIdx;
                action.TargetObject = objectsOnContainer[objectIdx];
                action.TargetMethod = null;
                action.Arguments = null;
            }

            var type = action.TargetObject.GetType();
            EnsureMethodsCached(type);

            var methods = typeToMethod[type];
            var currentIdx = methods.IndexOf(action.TargetMethod);
            var nameIdx = currentIdx + 1;

            var methodNames = typeToMethodNames[type];

            //Detect deleted method
            if (currentIdx == -1 && action.TargetMethod != null && !string.IsNullOrEmpty(action.TargetMethod.MethodName))
                methodNames[0] = "Deleted method! Old name: " + action.TargetMethod.MethodName;
            else
                methodNames[0] = NoMethodSelected;

            var methodSelectRect = NextPosition(objectSelectRect, EditorGUIUtility.singleLineHeight);
            var methodSelectRect_label = methodSelectRect;
            var methodSelectRect_popup = methodSelectRect;
            methodSelectRect_label.width = 90f;
            methodSelectRect_popup.xMin = methodSelectRect_label.xMax + 5f;

            EditorGUI.LabelField(methodSelectRect_label, "Target Method:");
            var newNameIdx = EditorGUI.Popup(methodSelectRect_popup, nameIdx, methodNames);

            if (newNameIdx != nameIdx)
            {
                if (newNameIdx == 0)
                {
                    action.TargetMethod = null;
                    action.Arguments = new SerializableArgument[0];
                }
                else
                {
                    var oldMethod = action.TargetMethod;
                    var targetMethod = methods[newNameIdx - 1];
                    action.TargetMethod = targetMethod;

                    var oldParameters = action.Arguments;
                    action.Arguments = new SerializableArgument[action.TargetMethod.ParameterTypes.Length];
                    for (int i = 0; i < action.Arguments.Length; i++)
                    {
                        object value;
                        if (oldMethod != null && oldParameters != null && i < oldParameters.Length &&
                            oldMethod.ParameterTypes[i].Equals(targetMethod.ParameterTypes[i]))
                        {
                            value = oldParameters[i].UnpackParameter();
                        }
                        else
                        {
                            value = DefaultValueFinder.CreateDefaultFor(targetMethod.ParameterTypes[i].SystemType);
                        }

                        action.Arguments[i] = new SerializableArgument(value,
                                                                       targetMethod.ParameterTypes[i].SystemType,
                                                                       targetMethod.ParameterNames[i]);
                    }
                }
            }

            if (newNameIdx == 0)
            {
                if (EditorGUI.EndChangeCheck())
                    property.SetDirty();
                return;
            }

            if (EditorGUI.EndChangeCheck())
                property.SetDirty();

            if (action.Arguments.Length == 0)
                return;

            position = NextPosition(position, EditorGUIUtility.singleLineHeight);
            EditorGUI.indentLevel += 2;

            //Updating the serializedObject here syncs it with the changes from above
//            property.serializedObject.Update();
            var parameters = property.FindPropertyRelative("arguments");
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var positionHeight = SerializableArgumentDrawer.GetHeightForType(action.Arguments[i].ParameterType.SystemType,
                                                                                  EditorGUIUtility.singleLineHeight);
                position = NextPosition(position, positionHeight);
                EditorGUI.PropertyField(position, parameters.GetArrayElementAtIndex(i), true);
            }
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel -= 2;
        }

        private static Rect NextPosition(Rect currentPosition, float nextPositionHeight)
        {
            currentPosition.y += currentPosition.height + 1;
            currentPosition.height = nextPositionHeight;
            return currentPosition;
        }

        private static void EnsureMethodsCached(Type type)
        {
            if (!typeToMethod.ContainsKey(type))
            {
                var methods = MethodLister.SerializeableMethodsOn(type);
                var methodNames = new string[methods.Count + 1];
                methodNames[0] = NoMethodSelected;
                for (int i = 0; i < methods.Count; i++)
                {
                    methodNames[i + 1] = methods[i].ToString();
                }

                typeToMethod[type] = methods;
                typeToMethodNames[type] = methodNames;
            }
        }

        private static void ReadyObjectSelectDropdown(Object containingObject, Object targetObject,
                                                      out Object[] objectsOnContainer, out string[] objectNames, out int currentSelectedIdx)
        {
            //Has dragged-and-dropped something else.
            if (!(containingObject is GameObject))
            {
                objectsOnContainer = new[] {containingObject};
                objectNames = new[] {containingObject.GetType().Name};
                currentSelectedIdx = 0;
                return;
            }

            var go = (GameObject) containingObject;
            var components = go.GetComponents<Component>();
            //This pukes garbage in the editor, but caching breaks if new components are added to the container
            objectsOnContainer = new Object[components.Length + 1];
            objectNames = new string[components.Length + 1];
            objectsOnContainer[0] = containingObject;
            objectNames[0] = containingObject.GetType().Name;
            for (int i = 0; i < components.Length; i++)
            {
                objectsOnContainer[i + 1] = components[i];
                objectNames[i + 1] = components[i].GetType().Name;
            }

            currentSelectedIdx = Array.IndexOf(objectsOnContainer, targetObject);
        }

    }
}