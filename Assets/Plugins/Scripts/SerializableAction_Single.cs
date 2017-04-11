using System;
using Fasterflect;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    /// <summary>
    /// This is a serialized wrapper around a single action that can be drawn in the inspector.
    /// </summary>
    [Serializable]
    public class SerializableAction_Single : ISerializationCallbackReceiver
    {
        [SerializeField]
        private SerializableArgument[] arguments;
        [SerializeField]
        private Object targetObject;
        [SerializeField]
        private SerializableActionTarget target;
        public UnityEventCallState callState;

        /// <summary>
        /// Object the serialized action will be called on.
        /// </summary>
        public Object TargetObject { get { return targetObject; } set { targetObject = value; } }

        /// <summary>
        /// The serialized action
        /// </summary>
        public SerializableActionTarget Target { get { return target; } set { target = value; } }

        /// <summary>
        /// Arguments the serialized action will be invoked with.
        /// </summary>
        public SerializableArgument[] Arguments { get { return arguments; } set { arguments = value; } }

        private object[] argumentList;
        private MethodInvoker methodInvoker;
        private AttributeSetter fieldSetter;
        private bool methodDeleted;

        public SerializableAction_Single(Object targetObject, SerializableMethod target, params object[] parameters)
        {
            Assert.AreEqual(target.ParameterTypes.Length, parameters.Length);
            for (var i = 0; i < target.ParameterTypes.Length; i++)
            {
                var parameterType = target.ParameterTypes[i];
                Assert.IsTrue((parameters[i] == null && !parameterType.SystemType.IsValueType)|| parameterType.SystemType.IsInstanceOfType(parameters[i]));
            }

            this.targetObject = targetObject;
            this.target = new SerializableActionTarget(target);
            arguments = new SerializableArgument[parameters.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = new SerializableArgument(parameters[i], parameters[i].GetType(), target.ParameterNames[i]);
            }
        }

        public SerializableAction_Single(Object targetObject, SerializableFieldSetter target, object parameter)
        {
            Assert.IsTrue((parameter == null && !target.FieldType.IsValueType) || target.FieldType.IsInstanceOfType(parameter));
            this.targetObject = targetObject;
            this.target = new SerializableActionTarget(target);
            arguments = new[] { new SerializableArgument(parameter, parameter.GetType(), "Field?") }; //@TODO: figure out name
        }

        public void Invoke()
        {
            if (callState == UnityEventCallState.RuntimeOnly && !Application.isPlaying)
                return;
            if (callState == UnityEventCallState.Off)
                return;

            if (!methodDeleted)
            {
                if (methodInvoker != null)
                    methodInvoker.Invoke(targetObject, argumentList);
                else
                    fieldSetter.Invoke(targetObject, argumentList[0]);
            }
            else
                Debug.LogWarning("Trying to invoke SerializableAction, but the serialized action target " + target.Name + " has been deleted!");
        }

        public void OnBeforeSerialize()
        { }

        public void OnAfterDeserialize()
        {
            argumentList = new object[arguments.Length];
            var paramTypeList = new Type[arguments.Length];
            for (int i = 0; i < argumentList.Length; i++)
            {
                argumentList[i] = arguments[i].UnpackParameter();
                paramTypeList[i] = arguments[i].ArgumentType;
            }

            methodDeleted = false;
            try
            {
                if (target.IsMethod)
                {
                    methodInvoker = TargetObject.GetType().DelegateForInvoke(Target.TargetMethod.MethodName, paramTypeList);
                }
                else
                {
                    TargetObject.GetType().DelegateForSetField(Target.TargetFieldSetter.FieldName);
                }
            }
            catch (MissingMethodException)
            {
                methodDeleted = true;
            }
            catch (MissingFieldException)
            {
                methodDeleted = true;
            }

        }
    }
}