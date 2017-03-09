using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{

    public SerializableSystemType sst;
    public SerializableMethod serMet;
    public SerializableAction serializableAction;

    [ContextMenu("SetUp")]
    public void SetUp()
    {
        serMet = new SerializableMethod(GetType().GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance));
        serializableAction.targetObject = this;
        serializableAction.targetMethod = serMet;
    }

    public void Foo()
    {
        Debug.Log("Foo!");
    }

    private void Start()
    {
        serializableAction.Invoke();
    }

}
