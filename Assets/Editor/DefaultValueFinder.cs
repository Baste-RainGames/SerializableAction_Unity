﻿using System;
using UnityEngine;

namespace SerializableActions.Internal
{
    public static class DefaultValueFinder
    {
        public static object CreateDefaultFor(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            if (type == typeof(AnimationCurve))
                return new AnimationCurve();
            return null;
        }
    }
}