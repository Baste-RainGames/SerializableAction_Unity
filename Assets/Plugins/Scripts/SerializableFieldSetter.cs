using System;
using System.Reflection;
using UnityEngine;

namespace SerializableActions.Internal
{
    [Serializable]
    public class SerializableFieldSetter
    {
        [SerializeField] private SerializableSystemType containingType;
        [SerializeField] private SerializableSystemType fieldType;
        [SerializeField] private string fieldName;
        [SerializeField] private FieldInfo _fieldInfo;

        public Type FieldType { get { return fieldType; } }
        public string FieldName { get { return fieldName; }}
        public FieldInfo FieldInfo { get { return _fieldInfo; }}

        public SerializableFieldSetter(FieldInfo field)
        {
            containingType = field.DeclaringType;
            fieldType = field.FieldType;
            fieldName = field.Name;
            _fieldInfo = field;
        }


        //@TODO: Actually serialize
    }
}