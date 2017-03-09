using System;
using UnityEngine;

[Serializable]
public class SerializeableParameterType : SerializableSystemType
{
    [SerializeField]
    private bool m_isGeneric;
    public bool IsGeneric { get { return m_isGeneric; } set { m_isGeneric = value; } }

    public SerializeableParameterType(Type _SystemType) : base(_SystemType)
    { }
}