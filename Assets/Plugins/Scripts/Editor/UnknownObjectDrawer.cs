using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerializableActions.Internal
{
    public static class UnknownObjectDrawer
    {
        private static Dictionary<Type, List<FieldInfo>> drawableFieldsForType = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, float> heightRequiredToDrawType = new Dictionary<Type, float>();

        public static object Draw(Rect position, GUIContent label, object o)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            if (o == null)
            {
                EditorGUI.LabelField(position, "Drawing null!");
                return o;
            }

            EditorGUI.LabelField(position, label);
            EditorGUI.indentLevel++;
            foreach (var field in DrawableFields(o.GetType()))
            {
                var fieldValue = field.GetValue(o);
                var valueType = field.FieldType;
                position = EditorUtil.NextPosition(position, EditorGUIUtility.singleLineHeight);
                var newValue = SerializableArgumentDrawer.DrawObjectOfType(position, new GUIContent(field.Name), valueType, fieldValue, false);
                field.SetValue(o, newValue);
            }
            EditorGUI.indentLevel--;

            return o;
        }

        public static float HeightRequiredToDraw(Type t)
        {
            if (!heightRequiredToDrawType.ContainsKey(t))
                CacheRequiredHeightFor(t);
            return heightRequiredToDrawType[t];
        }

        private static List<FieldInfo> DrawableFields(Type t)
        {
            if (!drawableFieldsForType.ContainsKey(t))
                CacheDrawableFieldsFor(t);
            return drawableFieldsForType[t];
        }

        private static void CacheDrawableFieldsFor(Type t)
        {
            var drawableFields = new List<FieldInfo>();

            var allFields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in allFields)
                drawableFields.Add(field);

            drawableFieldsForType[t] = drawableFields;
        }

        private static void CacheRequiredHeightFor(Type type)
        {
            heightRequiredToDrawType[type] =
                EditorGUIUtility.singleLineHeight +
                DrawableFields(type).Sum(field => SerializableArgumentDrawer.GetHeightForType(type, EditorGUIUtility.singleLineHeight, false));
        }
    }
}