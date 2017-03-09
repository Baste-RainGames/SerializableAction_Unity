// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using System;
using UnityEngine;

[System.Serializable]
public class SerializableSystemType
{
    [SerializeField]
    private string m_Name;

    public string Name { get { return m_Name; } }

    [SerializeField]
    private string m_AssemblyQualifiedName;

    public string AssemblyQualifiedName { get { return m_AssemblyQualifiedName; } }

    [SerializeField]
    private string m_AssemblyName;

    public string AssemblyName { get { return m_AssemblyName; } }

    private System.Type m_SystemType;
    public System.Type SystemType
    {
        get
        {
            if (m_SystemType == null)
            {
                GetSystemType();
            }
            return m_SystemType;
        }
    }

    private void GetSystemType()
    {
        m_SystemType = System.Type.GetType(m_AssemblyQualifiedName);
    }

    public SerializableSystemType(System.Type _SystemType)
    {
        m_SystemType = _SystemType;
        m_Name = _SystemType.Name;
        m_AssemblyQualifiedName = _SystemType.AssemblyQualifiedName;
        m_AssemblyName = _SystemType.Assembly.FullName;
    }

    public override bool Equals(System.Object obj)
    {
        SerializableSystemType temp = obj as SerializableSystemType;
        if ((object) temp == null)
        {
            return false;
        }
        return this.Equals(temp);
    }

    public bool Equals(SerializableSystemType _Object)
    {
        //return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
        return _Object.SystemType.Equals(SystemType);
    }

    public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object) a == null) || ((object) b == null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return SystemType.GetHashCode();
    }

    public static implicit operator SerializableSystemType(System.Type type)
    {
        return new SerializableSystemType(type);
    }

    public static implicit operator System.Type(SerializableSystemType type)
    {
        return type.SystemType;
    }
}

[System.Serializable]
public class SerializeableParameterType : SerializableSystemType
{
    [SerializeField]
    private bool m_isGeneric;
    public bool IsGeneric { get { return m_isGeneric; } set { m_isGeneric = value; } }

    public SerializeableParameterType(Type _SystemType) : base(_SystemType)
    { }
}