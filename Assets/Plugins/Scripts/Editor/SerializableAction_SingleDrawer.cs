using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    [CustomPropertyDrawer(typeof(SerializableAction_Single))]
    public class SerializableAction_SingleDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<SerializableMethod>> typeToMethods = new Dictionary<Type, List<SerializableMethod>>();
        private static readonly Dictionary<Type, List<SerializableFieldSetter>> typeToFields = new Dictionary<Type, List<SerializableFieldSetter>>();
        private static readonly Dictionary<Type, string[]> typeToNames = new Dictionary<Type, string[]>();
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
            position = EditorUtil.NextPosition(position, EditorGUIUtility.singleLineHeight);
            DrawSerializableAction(position, property);
            EditorGUI.indentLevel--;
        }

        public static float FindSerializableActionHeight(SerializedProperty property, GUIContent label)
        {
            float defaultHeight = EditorGUIUtility.singleLineHeight + 1;
            var action = (SerializableAction_Single) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

            if (action.TargetObject == null)
            {
                return defaultHeight * 2; //label + object field
            }

            //label + object field + method field. 1 less if there's no label
            var propHeightWithMethod = defaultHeight * (label == GUIContent.none ? 2 : 3);
            var target = action.Target;
            if (target == null || !target.HasTarget)
            {
                return propHeightWithMethod;
            }

            if (!target.IsMethod)
            {
                return propHeightWithMethod + SerializableArgumentDrawer.GetHeightForType(target.TargetFieldSetter.FieldType, defaultHeight, 0);
            }

            var targetMethod = target.TargetMethod;
            var parameterCount = targetMethod.ParameterTypes.Length;
            if (parameterCount == 0)
                return propHeightWithMethod;

            var finalPropHeight = propHeightWithMethod;
            for (var i = 0; i < targetMethod.ParameterTypes.Length; i++)
            {
                var parameterType = targetMethod.ParameterTypes[i];
                finalPropHeight += SerializableArgumentDrawer.GetHeightForType(parameterType.SystemType, defaultHeight, 0);
            }

            return finalPropHeight;
        }

        /*
         * This OnGUI mostly uses the raw object instead of fiddling with the SerializedProperty.
         * This is because we need to handle things like raw System.Objects and arrays,
         * both of which SerializedProperties can't deal with.
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

            var objectRect = EditorUtil.NextPosition(callStateRect, EditorGUIUtility.singleLineHeight);
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
                action.Target = null;
                action.Arguments = null;
            }

            var type = action.TargetObject.GetType();
            EnsureDataCached(type);

            var methods = typeToMethods[type];
            var fields = typeToFields[type];
            var methodAndFieldNames = typeToNames[type];

            int methodIdx = 0, fieldIdx = 0, nameIdx = 0;
            bool isDeleted = false;

            var target = action.Target;
            //@TODO: This might never be null due to Unity's inspector always initializing fields.
            if (target != null)
            {
                if (target.IsMethod)
                {
                    fieldIdx = -1;
                    methodIdx = target.HasTarget ? methods.IndexOf(target.TargetMethod) : -1;
                    nameIdx = methodIdx + 1;

                    isDeleted = methodIdx == -1 && !string.IsNullOrEmpty(target.TargetMethod.MethodName);
                }
                else
                {
                    methodIdx = -1;
                    fieldIdx = target.HasTarget ? fields.IndexOf(target.TargetFieldSetter) : -1;
                    nameIdx = fieldIdx == -1 ? 0 : fieldIdx + 1 + methods.Count;

                    isDeleted = fieldIdx == -1 && !string.IsNullOrEmpty(target.TargetFieldSetter.FieldName);
                }
            }

            //Detect deleted method
            if (isDeleted)
                methodAndFieldNames[0] = string.Format("Deleted {0}! Old name: {1}", target.IsMethod ? "Method" : "Field", target.Name);
            else
                methodAndFieldNames[0] = NoMethodSelected;

            var actionSelectRect = EditorUtil.NextPosition(objectSelectRect, EditorGUIUtility.singleLineHeight);
            var actionSelectRect_label = actionSelectRect;
            var actionSelectRect_popup = actionSelectRect;
            actionSelectRect_label.width = 90f;
            actionSelectRect_popup.xMin = actionSelectRect_label.xMax + 5f;

            EditorGUI.LabelField(actionSelectRect_label, "Target action:");
            var newNameIdx = EditorGUI.Popup(actionSelectRect_popup, nameIdx, methodAndFieldNames);

            if (newNameIdx != nameIdx)
            {
                //Selected "None", clear target method
                if (newNameIdx == 0)
                {
                    action.Target = null;
                    action.Arguments = new SerializableArgument[0];
                }
                //Selected a method or a setter
                else if (newNameIdx < methods.Count + 1)
                {
                    var oldMethod = action.Target.IsMethod ? action.Target.TargetMethod : null;
                    var targetMethod = methods[newNameIdx - 1];
                    action.Target.TargetMethod = targetMethod;

                    var oldParameters = action.Arguments;
                    action.Arguments = new SerializableArgument[targetMethod.ParameterTypes.Length];
                    for (int i = 0; i < action.Arguments.Length; i++)
                    {
                        object value;
                        if (oldMethod != null && oldParameters != null && i < oldParameters.Length &&
                            oldMethod.ParameterTypes[i].Equals(targetMethod.ParameterTypes[i]))
                            value = oldParameters[i].UnpackParameter();
                        else
                            value = DefaultValueFinder.CreateDefaultFor(targetMethod.ParameterTypes[i].SystemType);

                        action.Arguments[i] = new SerializableArgument(value,
                                                                       targetMethod.ParameterTypes[i].SystemType,
                                                                       targetMethod.ParameterNames[i]);
                    }
                }
                //Selected a field
                else
                {
                    var targetField = fields[newNameIdx - methods.Count - 1];
                    action.Target.TargetFieldSetter = targetField;
                    var arg = new SerializableArgument(DefaultValueFinder.CreateDefaultFor(targetField.FieldType),
                                                       targetField.FieldType,
                                                       "Value");
                    action.Arguments = new[] { arg };
                }
            }

            var changed = EditorGUI.EndChangeCheck();
            var noMethodSelected = newNameIdx == 0;
            if (changed || noMethodSelected || action.Arguments.Length == 0)
            {
                if (changed)
                    property.SetDirty();
                return;
            }

            position = EditorUtil.NextPosition(position, EditorGUIUtility.singleLineHeight);
            EditorGUI.indentLevel += 2;

            var parameters = property.FindPropertyRelative("arguments");
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var positionHeight = SerializableArgumentDrawer.GetHeightForType(action.Arguments[i].ParameterType.SystemType,
                                                                                 EditorGUIUtility.singleLineHeight, 0);
                position = EditorUtil.NextPosition(position, positionHeight);
                EditorGUI.PropertyField(position, parameters.GetArrayElementAtIndex(i), true);
            }
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel -= 2;
        }

        private static void EnsureDataCached(Type type)
        {
            if (!typeToMethods.ContainsKey(type))
            {
                var methods = MethodLister.SerializeableMethodsOn(type);
                var methodNames = new string[methods.Count + 1];
                methodNames[0] = NoMethodSelected;
                for (int i = 0; i < methods.Count; i++)
                {
                    methodNames[i + 1] = MethodName(methods[i]);
                }

                var fields = FieldUtil.SerializableFields(type);

                typeToMethods[type] = methods;
                typeToFields[type] = fields.Select(field => new SerializableFieldSetter(field)).ToList();
                typeToNames[type] = methodNames.Concat(fields.Select(ShowFieldName)).ToArray();
            }
        }

        private static string ShowFieldName(FieldInfo field)
        {
            return string.Format("Set {0} ({1})", field.Name, Util.PrettifyTypeName(field.FieldType.Name));
        }

        private static string MethodName(SerializableMethod method)
        {
            var methodInfo = method.MethodInfo;
            if (methodInfo == null)
                return "Null method";

            var parameterTypes = method.ParameterTypes;
            var parameterNames = method.ParameterNames;
            if (MethodLister.IsSetter(methodInfo))
            {
                var nameBuilder = new StringBuilder(methodInfo.Name.Replace("_", " "));
                nameBuilder[0] = 'S'; //Upper case "set"
                return string.Format("{0} ({1})", nameBuilder.ToString(), parameterTypes[0]);
            }

            string printData = methodInfo.Name;
            printData += "(";
            if (parameterTypes.Length > 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    printData += string.Format("{0} {1}", parameterTypes[i], parameterNames[i]);
                    if (i != parameterTypes.Length - 1)
                        printData += ", ";
                }
            }
            printData += ")";
            if (methodInfo.ReturnType != typeof(void))
            {
                printData += " => " + methodInfo.ReturnType.Name;
            }
            return printData;
        }

        private static void ReadyObjectSelectDropdown(Object containingObject, Object targetObject,
                                                      out Object[] objectsOnContainer, out string[] objectNames, out int currentSelectedIdx)
        {
            //Has dragged-and-dropped something else.
            if (!(containingObject is GameObject))
            {
                objectsOnContainer = new[] { containingObject };
                objectNames = new[] { containingObject.GetType().Name };
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