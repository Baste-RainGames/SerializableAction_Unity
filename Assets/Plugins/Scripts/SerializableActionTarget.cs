using System;
using UnityEngine;

namespace SerializableActions.Internal
{
    [Serializable]
    public class SerializableActionTarget
    {
        [SerializeField]
        private SerializableMethod _targetMethod;
        [SerializeField]
        private SerializableFieldSetter _targetFieldSetter;
        [SerializeField]
        private bool _isMethod;

        public SerializableActionTarget(SerializableMethod target)
        {
            TargetMethod = target;
        }

        public SerializableActionTarget(SerializableFieldSetter target)
        {
            TargetFieldSetter = target;
        }

        public SerializableMethod TargetMethod
        {
            get { return _targetMethod; }
            set
            {
                _targetMethod = value;
                _isMethod = true;
                _targetFieldSetter = null;
            }
        }

        public SerializableFieldSetter TargetFieldSetter
        {
            get { return _targetFieldSetter; }
            set
            {
                _targetFieldSetter = value;
                _isMethod = false;
                _targetMethod = null;
            }
        }

        public bool IsMethod { get { return _isMethod; } }
        public string Name { get { return IsMethod ? TargetMethod.MethodName : TargetFieldSetter.FieldName; } }
        public bool HasTarget { get { return IsMethod ? TargetMethod.MethodInfo != null : !string.IsNullOrEmpty(TargetFieldSetter.FieldName); } }
    }
}