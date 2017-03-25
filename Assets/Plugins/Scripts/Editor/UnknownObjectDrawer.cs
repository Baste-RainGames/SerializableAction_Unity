using UnityEditor;
using UnityEngine;

namespace SerializableActions.Internal
{
    public static class UnknownObjectDrawer
    {
        public static object Draw(Rect position, GUIContent label, object o, int currentDepth)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            if (o == null)
            {
                EditorGUI.LabelField(position, "Drawing null!");
                return o;
            }

            EditorGUI.LabelField(position, label);
            EditorGUI.indentLevel++;
            foreach (var field in FieldUtil.DrawableFields(o.GetType()))
            {
                var fieldValue = field.GetValue(o);
                var valueType = field.FieldType;
                position = EditorUtil.NextPosition(position, FieldUtil.HeightRequiredToDraw(field.FieldType));
                var newValue = SerializableArgumentDrawer.DrawObjectOfType(position, new GUIContent(field.Name), valueType, fieldValue, currentDepth + 1);
                field.SetValue(o, newValue);
            }
            EditorGUI.indentLevel--;

            return o;
        }
    }
}