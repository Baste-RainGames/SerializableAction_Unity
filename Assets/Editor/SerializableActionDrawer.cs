using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(SerializableAction))]
public class SerializableActionDrawer : PropertyDrawer
{

    private static Dictionary<Type, List<SerializableMethod>> typeToMethod = new Dictionary<Type, List<SerializableMethod>>();
    private static Dictionary<Type, string[]> typeToMethodNames = new Dictionary<Type, string[]>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var defaultHeight = base.GetPropertyHeight(property, label);
        var action = (SerializableAction) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

        if (action.TargetObject == null)
            return defaultHeight * 2;

        var propHeightWithMethod = defaultHeight * 3;
        if (action.TargetMethod == null || action.TargetMethod.MethodInfo == null)
            return propHeightWithMethod;

        var parameterCount = action.TargetMethod.ParameterTypes.Length;
        if (parameterCount == 0)
            return propHeightWithMethod;

        var finalPropHeight = propHeightWithMethod;
        for (var i = 0; i < action.TargetMethod.ParameterTypes.Length; i++)
        {
            var parameterType = action.TargetMethod.ParameterTypes[i];
            finalPropHeight += SerializableParameterDrawer.GetHeightForType(parameterType.SystemType, defaultHeight);
        }

        return finalPropHeight + 30;
    }

    /*
     * This OnGUI uses the raw object instead of fiddling with the SerializedProperty.
     * This is because we need to handle things like raw System.Objects and arrays,
     * both of which SerializedProperties can't handle properly.
     */
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.BeginChangeCheck();
        var action = (SerializableAction) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

        EditorGUI.LabelField(position, label);
        EditorGUI.indentLevel++;

        position = NextPosition(position, EditorGUIUtility.singleLineHeight);
        action.TargetObject = EditorGUI.ObjectField(position, "Target Object", action.TargetObject, typeof(Object), true);

        if (action.TargetObject == null)
        {
            if (EditorGUI.EndChangeCheck())
                property.SetDirty();
            EditorGUI.indentLevel--;
            return;
        }

        var type = action.TargetObject.GetType();
        EnsureMethodsCached(type);

        var methods = typeToMethod[type];
        var currentIdx = methods.IndexOf(action.TargetMethod);
        var nameIdx = currentIdx + 1;

        position = NextPosition(position, EditorGUIUtility.singleLineHeight);
        var methodNames = typeToMethodNames[type];

        //Detect deleted method
        if (currentIdx == -1 && action.TargetMethod != null && !string.IsNullOrEmpty(action.TargetMethod.MethodName))
            methodNames[0] = "Deleted method! Old name: " + action.TargetMethod.MethodName;
        else
            methodNames[0] = "None selected";

        var newNameIdx = EditorGUI.Popup(position, "Target Method", nameIdx, methodNames);

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
            EditorGUI.indentLevel--;
            return;
        }

        if (EditorGUI.EndChangeCheck())
            property.SetDirty();

        if (action.Arguments.Length == 0)
            return;

        position = NextPosition(position, EditorGUIUtility.singleLineHeight);
        EditorGUI.indentLevel++;
        EditorGUI.LabelField(position, "Parameters:");
        EditorGUI.indentLevel++;

        //Updating the serializedObject here syncs it with the changes from above
        property.serializedObject.Update();
        var parameters = property.FindPropertyRelative("arguments");
        for (int i = 0; i < parameters.arraySize; i++)
        {
            var positionHeight = SerializableParameterDrawer.GetHeightForType(action.Arguments[i].ParameterType.SystemType,
                                                                              EditorGUIUtility.singleLineHeight);
            position = NextPosition(position, positionHeight);
            EditorGUI.PropertyField(position, parameters.GetArrayElementAtIndex(i), true);
        }
        property.serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    private Rect NextPosition(Rect currentPosition, float nextPositionHeight)
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
            methodNames[0] = "None selected";
            for (int i = 0; i < methods.Count; i++)
            {
                methodNames[i + 1] = methods[i].ToString();
            }

            typeToMethod[type] = methods;
            typeToMethodNames[type] = methodNames;
        }
    }
}