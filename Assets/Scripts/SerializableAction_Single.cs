using System;
using Fasterflect;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal {
    /// <summary>
    /// This is a serialized wrapper around a single action that can be drawn in the inspector.
    /// It's recommended to use the SerializableAction instead, as it support an arbitrary number of arguments.
    /// </summary>
    [Serializable]
    public class SerializableAction_Single : ISerializationCallbackReceiver {

        [SerializeField]
        private SerializableArgument[] arguments;
        [SerializeField]
        private Object targetObject;
        [SerializeField]
        private SerializableMethod targetMethod;

        /// <summary>
        /// Object the serialized action will be called on.
        /// </summary>
        public Object TargetObject { get { return targetObject; } set { targetObject = value; } }

        /// <summary>
        /// The serialized action
        /// </summary>
        public SerializableMethod TargetMethod { get { return targetMethod; } set { targetMethod = value; } }

        /// <summary>
        /// Arguments the serialized action will be invoked with.
        /// </summary>
        public SerializableArgument[] Arguments { get { return arguments; } set { arguments = value; } }

        private object[] argumentList;
        private MethodInvoker invoker;
        private bool methodDeleted;

        public SerializableAction_Single(Object targetObject, SerializableMethod targetMethod, params object[] parameters) {
            Assert.AreEqual(targetMethod.ParameterTypes.Length, parameters.Length);

            this.targetObject = targetObject;
            this.targetMethod = targetMethod;
            arguments = new SerializableArgument[parameters.Length];
            for (int i = 0; i < arguments.Length; i++) {
                arguments[i] = new SerializableArgument(parameters[i], parameters[i].GetType(), targetMethod.ParameterNames[i]);
            }
        }

        public void Invoke() {
            if (!methodDeleted)
                invoker.Invoke(targetObject, argumentList);
            else
                Debug.LogWarning("Trying to invoke SerializableAction, but the serialized method " + targetMethod.MethodName + " has been deleted!");
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            argumentList = new object[arguments.Length];
            var paramTypeList = new Type[arguments.Length];
            for (int i = 0; i < argumentList.Length; i++) {
                argumentList[i] = arguments[i].UnpackParameter();
                paramTypeList[i] = arguments[i].ArgumentType;
            }

            methodDeleted = false;
            try {
                invoker = TargetObject.GetType().DelegateForInvoke(TargetMethod.MethodName, paramTypeList);
            }
            catch (MissingMethodException) {
                methodDeleted = true;
            }

        }
    }
}