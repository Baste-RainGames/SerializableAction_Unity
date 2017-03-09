using System;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class SerializableAction
{
    public SerializableAction(Object targetObject, SerializableMethod targetMethod, params object[] parameters)
    {
        m_targetObject = targetObject;
        m_targetMethod = targetMethod;
        m_parameters = new SerializableParameter[parameters.Length];
        for (int i = 0; i < m_parameters.Length; i++)
        {
            m_parameters[i] = new SerializableParameter(parameters[i], parameters[i].GetType());
        }
    }

    /// <summary>
    /// Object containing the event
    /// </summary>
    [SerializeField]
    private Object m_targetObject;

    /// <summary>
    /// Method to call
    /// </summary>
    [SerializeField]
    private SerializableMethod m_targetMethod;

    /// <summary>
    /// parameters to invoke the method with
    /// </summary>
    [SerializeField]
    private SerializableParameter[] m_parameters;

    /// <summary>
    /// Invokes the action
    /// </summary>
    public void Invoke()
    {
        var paramList = new object[m_parameters.Length];
        for (int i = 0; i < paramList.Length; i++)
        {
            paramList[i] = m_parameters[i].UnpackParameter();
        }

        m_targetMethod.MethodInfo.Invoke(m_targetObject, paramList);
    }

    public static object GetDefault(Type type)
    {
        if(type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}