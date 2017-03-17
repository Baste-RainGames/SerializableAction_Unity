using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerializableActions.Internal
{
    public static class MethodLister
    {
        public static List<SerializableMethod> SerializeableMethodsOn(Type type, MethodListerOptions options = MethodListerOptions.Default)
        {
            var serMethods = new List<SerializableMethod>();

            var publicMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in publicMethods)
            {
                if((options & MethodListerOptions.IncludeObsolete) == 0 && IsObsolete(method))
                    continue;
                if ((options & MethodListerOptions.IncludeGetters) == 0 && IsGetter(method))
                    continue;
                if ((options & MethodListerOptions.IncludeSetters) == 0 && IsSetter(method))
                    continue;
                if ((options & MethodListerOptions.NoReturnValue) == 0 && method.ReturnType == typeof(void))
                    continue;
                if ((options & MethodListerOptions.HasReturnValue) == 0 && method.ReturnType != typeof(void))
                    continue;
                if ((options & MethodListerOptions.IncludeGenerics) == 0 && method.IsGenericMethod)
                    continue;

                serMethods.Add(new SerializableMethod(method));
            }

            return serMethods;
        }

        public static bool IsGetter(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == info);
        }

        public static bool IsSetter(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == info);
        }

        public static bool IsObsolete(MethodInfo info)
        {
            MemberInfo mInfo = info;
            mInfo = (MemberInfo) GetGetterFor(info) ?? info;
            mInfo = (MemberInfo) GetSetterFor(info) ?? info;

            return mInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length > 0;
        }

        private static PropertyInfo GetGetterFor(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetGetMethod() == info);
        }

        private static PropertyInfo GetSetterFor(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetSetMethod() == info);
        }
    }

    [Flags]
    public enum MethodListerOptions
    {
        /// <summary>
        /// Should methods with a return value be included?
        /// </summary>
        HasReturnValue = 1 << 0,
        /// <summary>
        /// Should methods with no return value be included?
        /// </summary>
        NoReturnValue = 1 << 1,
        /// <summary>
        /// Should generic methods be included?
        /// </summary>
        IncludeGenerics = 1 << 2,
        /// <summary>
        /// Should property getter methods be included?
        /// </summary>
        IncludeGetters = 1 << 3,
        /// <summary>
        /// Should property getter methods be included?
        /// </summary>
        IncludeSetters = 1 << 4,
        /// <summary>
        /// Should deprecated methods (marked "obsolete") be included?
        /// </summary>
        IncludeObsolete = 1 << 5,

        Default = HasReturnValue | NoReturnValue | IncludeSetters
    }
}