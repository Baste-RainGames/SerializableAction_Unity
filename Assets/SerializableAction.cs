using System;
using System.Linq.Expressions;
using Fasterflect;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

[Serializable]
public class SerializableAction : ISerializationCallbackReceiver
{
    public SerializableAction(Object targetObject, SerializableMethod targetMethod, params object[] parameters)
    {
        Assert.AreEqual(targetMethod.ParameterTypes.Length, parameters.Length);

        m_targetObject = targetObject;
        m_targetMethod = targetMethod;
        m_parameters = new SerializableParameter[parameters.Length];
        for (int i = 0; i < m_parameters.Length; i++)
        {
            m_parameters[i] = new SerializableParameter(parameters[i], parameters[i].GetType(), targetMethod.ParameterNames[i]);
        }
    }

    /// <summary>
    /// Object containing the event
    /// </summary>
    [SerializeField]
    private Object m_targetObject;
    public Object TargetObject { get { return m_targetObject; } set { m_targetObject = value; }}

    /// <summary>
    /// Method to call
    /// </summary>
    [SerializeField]
    private SerializableMethod m_targetMethod;
    public SerializableMethod TargetMethod { get { return m_targetMethod; } set { m_targetMethod = value; }}

    /// <summary>
    /// parameters to invoke the method with
    /// </summary>
    [SerializeField]
    private SerializableParameter[] m_parameters;
    public SerializableParameter[] Parameters { get { return m_parameters; } set { m_parameters = value; }}

    private object[] paramList;
    private MethodInvoker invoker;

    public void Invoke()
    {
        invoker.Invoke(m_targetObject, paramList);
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        paramList = new object[m_parameters.Length];
        var paramTypeList = new Type[m_parameters.Length];
        for (int i = 0; i < paramList.Length; i++)
        {
            paramList[i] = m_parameters[i].UnpackParameter();
            paramTypeList[i] = m_parameters[i].ParameterValueType;
        }

        invoker = TargetObject.GetType().DelegateForInvoke(TargetMethod.MethodName, paramTypeList);

    }
}