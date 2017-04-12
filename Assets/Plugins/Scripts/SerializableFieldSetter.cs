using System;
using System.Reflection;
using UnityEngine;

namespace SerializableActions.Internal
{
    [Serializable]
    public class SerializableFieldSetter
    {
        [SerializeField]
        private SerializableSystemType containingType;
        [SerializeField]
        private SerializableSystemType fieldType;
        [SerializeField]
        private string fieldName;

        public Type FieldType { get { return fieldType; } }
        public string FieldName { get { return fieldName; } }

        public SerializableFieldSetter(FieldInfo field)
        {
            containingType = field.DeclaringType;
            fieldType = field.FieldType;
            fieldName = field.Name;
        }

        private bool Equals(SerializableFieldSetter other)
        {
            return Equals(containingType, other.containingType) &&
                   Equals(fieldType, other.fieldType) &&
                   string.Equals(fieldName, other.fieldName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableFieldSetter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (containingType != null ? containingType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fieldType != null ? fieldType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fieldName != null ? fieldName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}