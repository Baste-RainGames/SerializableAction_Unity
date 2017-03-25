using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    [CustomPropertyDrawer(typeof(SerializableArgument))]
    public class SerializableArgumentDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var parameter = (SerializableArgument) SerializedPropertyHelper.GetTargetObjectOfProperty(property);
            var type = parameter.ParameterType.SystemType;
            return GetHeightForType(type, EditorGUIUtility.singleLineHeight + 1f, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var argument = (SerializableArgument) SerializedPropertyHelper.GetTargetObjectOfProperty(property);
            var type = argument.ParameterType.SystemType;
            var obj = argument.UnpackParameter();
            label = new GUIContent(argument.ParameterType.NiceName + " " + argument.Name);
            EditorGUI.BeginChangeCheck();

            //Ideally, this if/else chain should be solved with a PropertyField, but it's not possible to generate a
            //SerializedProperty from an arbitrary System.Object-field, which is what UnpackParameter returns.

            obj = DrawObjectOfType(position, label, type, obj, true);

            if (EditorGUI.EndChangeCheck())
            {
                argument.SetArgumentValue(obj);
                property.SetDirty();
            }
        }

        public static float GetHeightForType(Type type, float defaultHeight, bool checkArbitraryTypes)
        {
            //nonstandard heights
            if (type == typeof(Bounds))
                return defaultHeight * 3f;
            if (type == typeof(Rect))
                return defaultHeight * 2f;

            if (type == typeof(Color))
                return defaultHeight;
            if (type == typeof(AnimationCurve))
                return defaultHeight;
            if (type.IsEnum)
                return defaultHeight;
            if (type == typeof(float))
                return defaultHeight;
            if (type == typeof(int))
                return defaultHeight;
            if (type == typeof(string))
                return defaultHeight;
            if (type == typeof(bool))
                return defaultHeight;
            if (type == typeof(Vector2))
                return defaultHeight;
            if (type == typeof(Vector3))
                return defaultHeight;
            if (type == typeof(Vector4))
                return defaultHeight;
            if (type == typeof(Quaternion))
                return defaultHeight;
            if (typeof(Object).IsAssignableFrom(type))
                return defaultHeight;

            return checkArbitraryTypes ? UnknownObjectDrawer.HeightRequiredToDraw(type) : defaultHeight;
        }

        public static object DrawObjectOfType(Rect position, GUIContent label, Type type, object obj, bool tryDrawUnknownTypes)
        {
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
            else if (type == typeof(Vector2))
                obj = EditorGUI.Vector2Field(position, label, (Vector2) obj);
            else if (type == typeof(Vector3))
                obj = EditorGUI.Vector3Field(position, label, (Vector3) obj);
            else if (type == typeof(Vector4))
                obj = EditorGUI.Vector4Field(position, label, (Vector4) obj);
            else if (type == typeof(Quaternion))
                //Is there a better way to do this?
                obj = Quaternion.Euler(EditorGUI.Vector3Field(position, label, ((Quaternion) obj).eulerAngles));
            else if (type == typeof(Rect))
                obj = EditorGUI.RectField(position, label, (Rect) obj);
            else if (typeof(Object).IsAssignableFrom(type))
                obj = EditorGUI.ObjectField(position, label, (Object) obj, type, true);
            else
            {
                if (tryDrawUnknownTypes)
                    obj = UnknownObjectDrawer.Draw(position, label, obj);
                else
                    EditorGUI.LabelField(position, "Can't draw object of type " + type);
            }

            return obj;
        }
    }

}