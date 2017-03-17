using System;
using UnityEngine;

namespace SerializableActions.Internal
{
    /// <summary>
    /// Serializable Parameter type, which contains both the type of the parameter, and an IsGeneric flag.
    /// </summary>
    [Serializable]
    public class SerializeableParameterType : SerializableSystemType
    {
        [SerializeField]
        private bool m_isGeneric;
        public bool IsGeneric { get { return m_isGeneric; } set { m_isGeneric = value; } }

        public SerializeableParameterType(Type _SystemType) : base(_SystemType)
        { }

        public override string ToString()
        {
            return Name;
        }
    }
}