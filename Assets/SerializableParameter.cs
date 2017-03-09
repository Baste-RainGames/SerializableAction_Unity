using UnityEngine;
using FullSerializer;

[System.Serializable]
public class SerializableParameter
{
    private static readonly fsSerializer _serializer = new fsSerializer();


    [SerializeField]
    private string paramAsJson;
    [SerializeField]
    private SerializableSystemType parameterType;
    private object parameter;

    public SerializableParameter(object o, System.Type type)
    {
        parameter = o;
        parameterType = type;

        fsData data;
        _serializer.TrySerialize(type, o, out data).AssertSuccess();

        paramAsJson = fsJsonPrinter.PrettyJson(data);
    }

    public object UnpackParameter()
    {
        if (parameter == null)
        {
            var parsed = fsJsonParser.Parse(paramAsJson);
            _serializer.TryDeserialize(parsed, parameterType.SystemType, ref parameter).AssertSuccess();
        }
        return parameter;
    }
}