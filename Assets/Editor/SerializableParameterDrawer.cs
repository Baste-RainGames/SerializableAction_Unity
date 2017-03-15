using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(SerializableArgument))]
public class SerializableParameterDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var parameter = (SerializableArgument) SerializedPropertyHelper.GetTargetObjectOfProperty(property);
        var type = parameter.ParameterType.SystemType;
        return GetHeightForType(type, base.GetPropertyHeight(property, label));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var parameter = (SerializableArgument) SerializedPropertyHelper.GetTargetObjectOfProperty(property);
        var type = parameter.ParameterType.SystemType;
        var obj = parameter.UnpackParameter();
        label = new GUIContent(parameter.Name);
        EditorGUI.BeginChangeCheck();

        //Ideally, this if/else chain should be solved with a PropertyField, but it's not possible to generate a
        //SerializedProperty from an arbitrary object

        if (type == typeof(Bounds))
            obj = EditorGUI.BoundsField(position, label, (Bounds) obj);
        else if (type == typeof(Color))
            obj = EditorGUI.ColorField(position, label, (Color) obj);
        else if (type == typeof(AnimationCurve))
            obj = EditorGUI.CurveField(position, label, (AnimationCurve) obj);
        else if (type.IsEnum)
            if (type.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0)
                obj = EditorGUI.EnumMaskPopup(position, label, (Enum) obj);
            else
                obj = EditorGUI.EnumPopup(position, label, (Enum) obj);
        else if (type == typeof(float))
            obj = EditorGUI.FloatField(position, label, (float) obj);
        else if (type == typeof(int))
            obj = EditorGUI.IntField(position, label, (int) obj);
        else if (type == typeof(string))
            obj = EditorGUI.TextField(position, label, (string) obj);
        else if (type == typeof(bool))
            obj = EditorGUI.Toggle(position, label, (bool) obj);
        else if (type == typeof(Rect))
            obj = EditorGUI.RectField(position, label, (Rect) obj);
        else if (typeof(Object).IsAssignableFrom(type))
            obj = EditorGUI.ObjectField(position, label, (Object) obj, type, true);
        else
            EditorGUI.LabelField(position, "Can't create drawer for type " + type.Name);

        if (EditorGUI.EndChangeCheck())
        {
            parameter.SetArgumentValue(obj);
            property.SetDirty();
        }
    }

    public static float GetHeightForType(Type type, float defaultHeight)
    {
        if (type == typeof(Bounds))
            return defaultHeight * 3f;
        if (type == typeof(Rect))
            return defaultHeight * 2f;

        return defaultHeight;
    }
}