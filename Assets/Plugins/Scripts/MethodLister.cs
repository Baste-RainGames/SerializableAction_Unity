using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace SerializableActions.Internal
{
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

        /// <summary>
        /// Should the set of Invoke methods from MonoBehaviour be included? (Invoke, isInvoking, InvokeRepeating, etc.)
        /// </summary>
        IncludeInvokes = 1 << 6,

        /// <summary>
        /// Should the start/stop coroutine calls from MonoBehaviour be included?
        /// </summary>
        IncludeCoroutines = 1 << 7,

        /// <summary>
        /// Should the properties useGUILayout, runInEditMode, and hideFlags be included?
        /// </summary>
        IncludeUnityEditTimeProperties = 1 << 8,

        /// <summary>
        /// Should all of Unity's messaging methods be included (SendMessage, BroadcastMessage, SendMessageUpwards)
        /// </summary>
        IncludeSendMessage = 1 << 9,

        Default = NoReturnValue | IncludeSetters
    }

    public static class MethodLister
    {
        private static List<MethodInfo> invokeMethods;
        private static List<MethodInfo> coroutineMethods;
        private static List<MethodInfo> unityEditTimeGetSets;
        private static List<MethodInfo> sendMessageMethods;
        private static List<MethodInfo> neverIncludes;

        public static List<SerializableMethod> SerializeableMethodsOn(Type type, MethodListerOptions options = MethodListerOptions.Default)
        {
            var serMethods = new List<SerializableMethod>();

            var publicMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            foreach (var method in publicMethods)
            {
                if (NeverInclude(method))
                    continue;
                if ((options & MethodListerOptions.HasReturnValue) == 0 && method.ReturnType != typeof(void))
                {
                    //If we're including coroutines, accept StartCoroutine even if it has a return value
                    if ((options & MethodListerOptions.IncludeCoroutines) == 0 || !IsCoroutineMethod(method))
                        continue;
                }
                if ((options & MethodListerOptions.NoReturnValue) == 0 && method.ReturnType == typeof(void))
                {
                    //If we're including coroutines, accept StopCoroutine even if it has no value
                    if ((options & MethodListerOptions.IncludeCoroutines) == 0 || !IsCoroutineMethod(method))
                        continue;
                }
                if ((options & MethodListerOptions.IncludeGenerics) == 0 && method.IsGenericMethod)
                    continue;
                if ((options & MethodListerOptions.IncludeGetters) == 0 && IsGetter(method))
                    continue;
                if ((options & MethodListerOptions.IncludeSetters) == 0 && IsSetter(method))
                    continue;
                if ((options & MethodListerOptions.IncludeObsolete) == 0 && IsObsolete(method))
                    continue;
                if ((options & MethodListerOptions.IncludeInvokes) == 0 && IsInvokeMethod(method))
                    continue;
                if ((options & MethodListerOptions.IncludeCoroutines) == 0 && IsCoroutineMethod(method))
                    continue;
                if ((options & MethodListerOptions.IncludeUnityEditTimeProperties) == 0 && IsUnityEditTimeProp(method))
                    continue;
                if ((options & MethodListerOptions.IncludeSendMessage) == 0 && IsSendMessage(method))
                    continue;

                serMethods.Add(new SerializableMethod(method));
            }

            return serMethods;
        }

        private static bool NeverInclude(MethodInfo method)
        {
            if (neverIncludes == null)
            {
                neverIncludes = new List<MethodInfo>
                {
                    typeof(MonoBehaviour).GetMethod("GetComponents", new[] { typeof(Type), typeof(List<Component>) })
                };
            }

            return neverIncludes.Any(neverInclude => neverInclude.MethodHandle == method.MethodHandle);
        }

        public static bool IsGetter(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == info);
        }

        public static bool IsSetter(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == info);
        }

        private static PropertyInfo GetGetterFor(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetGetMethod() == info);
        }

        private static PropertyInfo GetSetterFor(MethodInfo info)
        {
            return info.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetSetMethod() == info);
        }

        public static bool IsObsolete(MethodInfo info)
        {
            MemberInfo mInfo = info;
            mInfo = (MemberInfo) GetGetterFor(info) ?? info;
            mInfo = (MemberInfo) GetSetterFor(info) ?? info;

            return mInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length > 0;
        }

        private static bool IsInvokeMethod(MethodInfo method)
        {
            if (invokeMethods == null)
            {
                invokeMethods = new List<MethodInfo>
                {
                    typeof(MonoBehaviour).GetMethod("Invoke"),
                    typeof(MonoBehaviour).GetMethod("InvokeRepeating"),
                    typeof(MonoBehaviour).GetMethod("CancelInvoke", new Type[0]),
                    typeof(MonoBehaviour).GetMethod("CancelInvoke", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("IsInvoking", new Type[0]),
                    typeof(MonoBehaviour).GetMethod("IsInvoking", new[] { typeof(string) })
                };
            }

            return invokeMethods.Any(invokeMethod => invokeMethod.MethodHandle == method.MethodHandle);
        }

        private static bool IsCoroutineMethod(MethodInfo method)
        {
            if (coroutineMethods == null)
            {
                coroutineMethods = new List<MethodInfo>
                {
                    typeof(MonoBehaviour).GetMethod("StartCoroutine", new[] { typeof(IEnumerator) }),
                    typeof(MonoBehaviour).GetMethod("StartCoroutine", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("StartCoroutine", new[] { typeof(string), typeof(UnityEngine.Object) }),

                    typeof(MonoBehaviour).GetMethod("StopCoroutine", new[] { typeof(IEnumerator) }),
                    typeof(MonoBehaviour).GetMethod("StopCoroutine", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("StopCoroutine", new[] { typeof(Coroutine) }),

                    typeof(MonoBehaviour).GetMethod("StopAllCoroutines"),
                };
            }

            return coroutineMethods.Any(coroutineMethod => coroutineMethod.MethodHandle == method.MethodHandle);
        }

        private static bool IsUnityEditTimeProp(MethodInfo method)
        {
            if (unityEditTimeGetSets == null)
            {
                unityEditTimeGetSets = new List<MethodInfo>
                {
                    typeof(MonoBehaviour).GetProperty("runInEditMode").GetGetMethod(),
                    typeof(MonoBehaviour).GetProperty("runInEditMode").GetSetMethod(),
                    typeof(MonoBehaviour).GetProperty("useGUILayout").GetGetMethod(),
                    typeof(MonoBehaviour).GetProperty("useGUILayout").GetSetMethod(),
                    typeof(MonoBehaviour).GetProperty("hideFlags").GetGetMethod(),
                    typeof(MonoBehaviour).GetProperty("hideFlags").GetSetMethod()
                };
            }

            return unityEditTimeGetSets.Any(editTimeGetSet => editTimeGetSet.MethodHandle == method.MethodHandle);
        }

        private static bool IsSendMessage(MethodInfo method)
        {
            if (sendMessageMethods == null)
            {
                sendMessageMethods = new List<MethodInfo>
                {
                    typeof(MonoBehaviour).GetMethod("SendMessage", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("SendMessage", new[] { typeof(string), typeof(object) }),
                    typeof(MonoBehaviour).GetMethod("SendMessage", new[] { typeof(string), typeof(SendMessageOptions) }),
                    typeof(MonoBehaviour).GetMethod("SendMessage", new[] { typeof(string), typeof(object), typeof(SendMessageOptions) }),

                    typeof(MonoBehaviour).GetMethod("SendMessageUpwards", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("SendMessageUpwards", new[] { typeof(string), typeof(object) }),
                    typeof(MonoBehaviour).GetMethod("SendMessageUpwards", new[] { typeof(string), typeof(SendMessageOptions) }),
                    typeof(MonoBehaviour).GetMethod("SendMessageUpwards", new[] { typeof(string), typeof(object), typeof(SendMessageOptions) }),

                    typeof(MonoBehaviour).GetMethod("BroadcastMessage", new[] { typeof(string) }),
                    typeof(MonoBehaviour).GetMethod("BroadcastMessage", new[] { typeof(string), typeof(object) }),
                    typeof(MonoBehaviour).GetMethod("BroadcastMessage", new[] { typeof(string), typeof(SendMessageOptions) }),
                    typeof(MonoBehaviour).GetMethod("BroadcastMessage", new[] { typeof(string), typeof(object), typeof(SendMessageOptions) }),
                };

            }

            return sendMessageMethods.Any(sendMessageMethod => sendMessageMethod.MethodHandle == method.MethodHandle);
        }
    }
}