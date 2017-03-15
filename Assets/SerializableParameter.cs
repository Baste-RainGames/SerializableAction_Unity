using System;
using FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

[Serializable]
public class SerializableParameter
{
    private static readonly fsSerializer _serializer = new fsSerializer();

    [SerializeField]
    private string parameterName;
    /// <summary>
    /// Name of the parameter in the method declaration. Only used for display purposes
    /// </summary>
    public string Name { get { return parameterName; } }

    [SerializeField]
    private SerializableSystemType declaredParameterType;
    /// <summary>
    /// The type of the parameter in the method declaration
    /// </summary>
    public SerializableSystemType DeclaredParameterType { get { return declaredParameterType; } }

    [SerializeField]
    private SerializableSystemType parameterValueType;
    /// <summary>
    /// The type of the serialized parameter. Is either DeclaredParameterType, a subtype of DeclaredParameterType
    /// </summary>
    public SerializableSystemType ParameterValueType { get { return parameterValueType; } }

    /// <summary>
    /// Is the object a UnityEngine.Object? In that case, we insert it into a ScriptableObject for serialization.
    /// </summary>
    [SerializeField]
    private bool isUnityObject;

    /// <summary>
    /// Serialized JSON version of the parameter. This is how the parameter is serialized if it is not an UnityEngine.Object.
    /// If the parameter is a UnityEngine.Object, this is an empty string
    /// </summary>
    [SerializeField]
    private string paramAsJson;

    /// <summary>
    /// A wrapper for the parameter. This is how the parameter is serialized if it is an UnityEngine.Object.
    /// If the parameter is something else, this is null
    /// </summary>
    [SerializeField]
    private UnityEngineObjectWrapper objectWrapper;

    /// <summary>
    /// The runtime value of the parameter.
    /// </summary>
    private object parameterValue;

    public SerializableParameter(object argument, Type type, string name)
    {
        declaredParameterType = type;
        parameterValueType = argument == null ? type : argument.GetType();
        parameterName = name;

        SetParameterValue(argument);
    }

    public object UnpackParameter()
    {
        if (parameterValue == null)
        {
            if (isUnityObject)
            {
                parameterValue = objectWrapper.objectReference;
            }
            else
            {
                var parsed = fsJsonParser.Parse(paramAsJson);
                _serializer.TryDeserialize(parsed, parameterValueType.SystemType, ref parameterValue).AssertSuccess();
            }
        }
        return parameterValue;
    }

    public void SetParameterValue(object o)
    {
        if (declaredParameterType.SystemType.IsValueType)
        {
            if (o == null)
                throw new ArgumentException("Trying to set null as parameter value for a SerializeableParameter, but the parameter's type is the "+
                                            "value type " + declaredParameterType.Name + "!");

            if (o.GetType() != declaredParameterType.SystemType)
                throw new ArgumentException("Trying to assign " + o + " of value type " + o.GetType() + " to a SerializeableParameter that expects a value"+
                                            "of value type " + declaredParameterType.Name);
        }
        else
        {
            if (o != null && !declaredParameterType.SystemType.IsInstanceOfType(o))
                throw new ArgumentException("Trying to assign " + o + " of type " + o.GetType() + " to SerializeableParameter expecting type " +
                                            declaredParameterType.Name + ", which is not valid! Use the type or a subtype");
        }

        if (o == null)
            parameterValueType = declaredParameterType;
        else
            parameterValueType = o.GetType();

        isUnityObject = o is Object;
        if (isUnityObject)
        {
            paramAsJson = "";
            objectWrapper = ScriptableObject.CreateInstance<UnityEngineObjectWrapper>();
            objectWrapper.objectReference = o as UnityEngine.Object;
        }
        else
        {
            if (objectWrapper != null)
            {
                if(Application.isPlaying)
                    Object.Destroy(objectWrapper);
                else
                    Object.DestroyImmediate(objectWrapper);
            }

            objectWrapper = null;
            parameterValue = o;
            fsData data;
            _serializer.TrySerialize(parameterValueType, o, out data).AssertSuccess();
            paramAsJson = fsJsonPrinter.CompressedJson(data);
        }
    }

}