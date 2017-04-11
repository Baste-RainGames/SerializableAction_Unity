using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    public static class FieldUtil
    {
        private static Dictionary<Type, List<FieldInfo>> drawableFieldsForType = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, float> heightRequiredToDrawType = new Dictionary<Type, float>();

        private static Dictionary<Type, List<FieldInfo>> serializableFieldsForType = new Dictionary<Type, List<FieldInfo>>();

        public static float HeightRequiredToDraw(Type t, int currentRecursionDepth = 0)
        {
            if (!heightRequiredToDrawType.ContainsKey(t))
                CacheRequiredHeightFor(t, currentRecursionDepth);
            return heightRequiredToDrawType[t];
        }

        public static List<FieldInfo> SerializableFields(Type t)
        {
            if (!serializableFieldsForType.ContainsKey(t))
                CacheSerializableFieldsFor(t);
            return serializableFieldsForType[t];
        }

        public static List<FieldInfo> DrawableFields(Type t)
        {
            if (!drawableFieldsForType.ContainsKey(t))
                CacheDrawableFieldsFor(t);
            return drawableFieldsForType[t];
        }

        private static void CacheDrawableFieldsFor(Type type)
        {
            drawableFieldsForType[type] = FieldIterator(type)
                .Where(field =>
                       {
                           var fieldType = field.FieldType;
                           if (fieldType == type)
                               return false; //No recursive types!
                           if (fieldType == typeof(SerializableAction) || fieldType == typeof(SerializableAction_Single))
                               return false;

                           return true;
                       })
                .ToList();
        }

        private static void CacheSerializableFieldsFor(Type type)
        {
            serializableFieldsForType[type] =
                FieldIterator(type)
                    .Where(field => IsSerializable(field))
                    .ToList();
        }

        private static void CacheRequiredHeightFor(Type type, int currentRecursionDepth)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight + 1;

            heightRequiredToDrawType[type] =
                lineHeight +
                DrawableFields(type).Sum(field => SerializableArgumentDrawer.GetHeightForType(field.FieldType, lineHeight, currentRecursionDepth));

        }

        private static bool IsSerializable(FieldInfo field, int recusionDepth = 0)
        {
            Debug.Log("Checking field " + field.Name);

            if (recusionDepth >= 7)
                return false;
            if (field == null)
                return false;

            var type = field.FieldType;

            if (type == typeof(SerializableAction))
                return false;
            if (type == typeof(SerializableAction_Single))
                return false;

            if (type == typeof(Bounds))
                return true;
            if (type == typeof(Color))
                return true;
            if (type == typeof(AnimationCurve))
                return true;
            if (type.IsEnum)
                return true;
            if (type == typeof(float))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(bool))
                return true;
            if (type == typeof(Vector2))
                return true;
            if (type == typeof(Vector3))
                return true;
            if (type == typeof(Vector4))
                return true;
            if (type == typeof(Quaternion))
                return true;
            if (type == typeof(Rect))
                return true;
            if (typeof(Object).IsAssignableFrom(type))
                return true;

            return type.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0;
        }

        private static IEnumerable<FieldInfo> FieldIterator(Type t)
        {
            var typeStack = GetTypeStack(t);

            while (typeStack.Count > 0)
            {
                var current = typeStack.Pop();
                var allFields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in allFields)
                {
                    if (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), true).Length > 0)
                        yield return field;
                }
            }
        }

        private static Stack<Type> GetTypeStack(Type t)
        {
            var typeStack = new Stack<Type>();
            var current = t;
            while (current != null)
            {
                typeStack.Push(current);
                current = current.BaseType;
            }
            return typeStack;
        }
    }
}