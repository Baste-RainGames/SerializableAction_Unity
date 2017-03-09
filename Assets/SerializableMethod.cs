using System;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class SerializableMethod
{
    [SerializeField]
    private string m_Name;
    [SerializeField]
    private SerializableSystemType m_containingType;
    [SerializeField]
    private BindingFlags m_bindingFlags;

    public SerializableMethod(MethodInfo method)
    {
        m_Name = method.Name;
        m_containingType = new SerializableSystemType(method.DeclaringType);
        m_MethodInfo = method;

        m_bindingFlags = method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        m_bindingFlags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
    }

    /// <summary>
    /// @TODO: Replace this with a delegate. See:
    /// http://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
    /// http://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue
    /// </summary>
    private MethodInfo m_MethodInfo;
    public MethodInfo MethodInfo
    {
        get
        {
            if (m_MethodInfo == null)
            {
                m_MethodInfo = m_containingType.SystemType.GetMethod(m_Name, m_bindingFlags);
            }
            return m_MethodInfo;
        }
    }
}