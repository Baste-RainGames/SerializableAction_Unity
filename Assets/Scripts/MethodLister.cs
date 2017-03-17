using System;
using System.Collections.Generic;
using System.Reflection;

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
                if (method.ReturnType == typeof(void) && (options & MethodListerOptions.NoReturnValue) == 0)
                    continue;
                if (method.ReturnType != typeof(void) && (options & MethodListerOptions.HasReturnValue) == 0)
                    continue;
                if (method.IsGenericMethod && (options & MethodListerOptions.IncludeGenerics) == 0)
                    continue;

                serMethods.Add(new SerializableMethod(method));
            }

            return serMethods;
        }
    }

    [Flags]
    public enum MethodListerOptions
    {
        HasReturnValue = 1 << 0,
        NoReturnValue = 1 << 1,
        IncludeGenerics = 1 << 2,

        Default = HasReturnValue | NoReturnValue | IncludeGenerics
    }
}