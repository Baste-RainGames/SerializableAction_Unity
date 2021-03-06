﻿using System;
using System.Linq;
using Fasterflect;
using FullSerializer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    public static class DefaultValueFinder
    {
        private static fsData emptyfsJsonData = fsJsonParser.Parse("{}");
        private static fsSerializer deserializer = new fsSerializer();

        public static object CreateDefaultFor(Type type, int depth = 0)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            if (type == typeof(AnimationCurve))
                return new AnimationCurve();
            if (type == typeof(Color))
                return Color.white;
            if (typeof(Object).IsAssignableFrom(type))
                return null;

            object parseResult = null;
            try
            {
                deserializer.TryDeserialize(emptyfsJsonData, type, ref parseResult).AssertSuccess();
            }
            catch (Exception)
            {
                //Deserialization failed, parseResult is now garbage.
                return null;
            }

            if (depth < SerializableSystemType.MaxSerializationDepth)
            {
                var internalFields = FieldUtil.DrawableFields(parseResult.GetType());
                foreach (var field in internalFields)
                {
                    field.SetValue(parseResult, CreateDefaultFor(field.FieldType, depth + 1));
                }
            }

            return parseResult;
        }
    }
}