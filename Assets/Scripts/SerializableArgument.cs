using System;
using FullSerializer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerializableActions.Internal
{
    /// <summary>
    /// This is a serializable wrapper for an argument. It contains both the value of the argument,
    /// and information about the parameter the argument is for.
    /// </summary>
    [Serializable]
    public class SerializableArgument
    {
        private static readonly fsSerializer _serializer = new fsSerializer();

        [SerializeField]
        private string parameterName;
        [SerializeField]
        private SerializableSystemType parameterType;
        [SerializeField]
        private SerializableSystemType argumentType;

        /// <summary>
        /// Name of the parameter in the method declaration. Only used for display purposes
        /// </summary>
        public string Name { get { return parameterName; } }

        /// <summary>
        /// The type of the parameter in the method declaration
        /// </summary>
        public SerializableSystemType ParameterType { get { return parameterType; } }

        /// <summary>
        /// The type of the serialized argument. It's always assignable to ParameterType
        /// </summary>
        public SerializableSystemType ArgumentType { get { return argumentType; } }

        /// <summary>
        /// Is the argument a UnityEngine.Object? In that case, we insert it into a ScriptableObject for serialization.
        /// </summary>
        [SerializeField]
        private bool isUnityObject;

        /// <summary>
        /// Serialized JSON version of the argument. This is how the argument is serialized if it is not an UnityEngine.Object.
        /// If the argument is a UnityEngine.Object, this is an empty string
        /// </summary>
        [SerializeField]
        private string paramAsJson;

        /// <summary>
        /// A wrapper for the argument. This is how the argument is serialized if it is an UnityEngine.Object.
        /// If the argument is something else, this is null
        /// </summary>
        [SerializeField]
        private UnityEngineObjectWrapper objectWrapper;

        /// <summary>
        /// The runtime value of the argument.
        /// </summary>
        private object argumentValue;

        public SerializableArgument(object argument, Type parameterType, string parameterName)
        {
            this.parameterType = parameterType;
            this.parameterName = parameterName;

            argumentType = argument == null ? parameterType : argument.GetType();
            SetArgumentValue(argument);
        }

        /// <returns>The runtime value of the argument</returns>
        public object UnpackParameter()
        {
            if (argumentValue == null)
            {
                if (isUnityObject)
                {
                    argumentValue = objectWrapper.objectReference;
                }
                else
                {
                    var parsed = fsJsonParser.Parse(paramAsJson);
                    _serializer.TryDeserialize(parsed, argumentType.SystemType, ref argumentValue).AssertSuccess();
                }
            }
            return argumentValue;
        }

        /// <summary>
        /// Set the value of the serialized argument.
        /// Note that this runs the serialization process, so it is not fast.
        /// </summary>
        /// <param name="argument">Argument to set</param>
        /// <exception cref="ArgumentException">If the argument is not assignable to the parameter type</exception>
        public void SetArgumentValue(object argument)
        {
            if (parameterType.SystemType.IsValueType)
            {
                if (argument == null)
                    throw new ArgumentException("Trying to set null as parameter value for a SerializeableParameter, but the parameter's type is the " +
                                                "value type " + parameterType.Name + "!");

                if (argument.GetType() != parameterType.SystemType)
                    throw new ArgumentException("Trying to assign " + argument + " of value type " + argument.GetType() +
                                                " to a SerializeableParameter that expects a value" +
                                                "of value type " + parameterType.Name);
            }
            else
            {
                if (argument != null && !parameterType.SystemType.IsInstanceOfType(argument))
                    throw new ArgumentException("Trying to assign " + argument + " of type " + argument.GetType() +
                                                " to SerializeableParameter expecting type " +
                                                parameterType.Name + ", which is not valid! Use the type or a subtype");
            }

            if (argument == null)
                argumentType = parameterType;
            else
                argumentType = argument.GetType();

            argumentValue = argument;
            isUnityObject = argument is Object;
            if (isUnityObject)
            {
                paramAsJson = "";
                objectWrapper = ScriptableObject.CreateInstance<UnityEngineObjectWrapper>();
                objectWrapper.objectReference = argument as UnityEngine.Object;
            }
            else
            {
                if (objectWrapper != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(objectWrapper);
                    else
                        Object.DestroyImmediate(objectWrapper);
                }

                objectWrapper = null;
                fsData data;
                _serializer.TrySerialize(argumentType, argument, out data).AssertSuccess();
                paramAsJson = fsJsonPrinter.CompressedJson(data);
            }
        }

    }
}