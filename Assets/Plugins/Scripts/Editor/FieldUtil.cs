using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerializableActions.Internal
{
    public static class FieldUtil
    {
        private static Dictionary<Type, List<FieldInfo>> drawableFieldsForType = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, float> heightRequiredToDrawType = new Dictionary<Type, float>();

        public static float HeightRequiredToDraw(Type t, int currentRecursionDepth = 0)
        {
            if (!heightRequiredToDrawType.ContainsKey(t))
                CacheRequiredHeightFor(t, currentRecursionDepth);
            return heightRequiredToDrawType[t];
        }

        public static List<FieldInfo> DrawableFields(Type t)
        {
            if (!drawableFieldsForType.ContainsKey(t))
                CacheDrawableFieldsFor(t);
            return drawableFieldsForType[t];
        }

        private static void CacheDrawableFieldsFor(Type t)
        {
            var drawableFields = new List<FieldInfo>();
            var typeStack = new Stack<Type>();
            var current = t;
            while (current != null)
            {
                typeStack.Push(current);
                current = current.BaseType;
            }

            while (typeStack.Count > 0)
            {
                current = typeStack.Pop();
                var allFields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in allFields)
                {
                    if(field.FieldType == t)
                        continue; //No recursive types!

                    if (field.IsPublic)
                        drawableFields.Add(field);
                    else if (field.GetCustomAttributes(typeof(SerializeField), true).Length > 0)
                        drawableFields.Add(field);

                }
            }

            drawableFieldsForType[t] = drawableFields;
        }

        private static void CacheRequiredHeightFor(Type type, int currentRecursionDepth)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight + 1;

            heightRequiredToDrawType[type] =
                lineHeight +
                DrawableFields(type).Sum(field => SerializableArgumentDrawer.GetHeightForType(field.FieldType, lineHeight, currentRecursionDepth));

        }
    }
}