using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour
{
    public SerializableAction serializableAction;

    public int sum;

    public void Foo(Person p)
    {
        sum += p.Age + p.Name.Length;
    }

    public void RunTest()
    {
        serializableAction.Invoke();
    }

}

[Serializable]
public class Person
{
    [SerializeField]
    private string name;
    [SerializeField]
    private int age;

    public string Name { get { return name; } }
    public int Age { get { return Age; } }

    public Person(string name, int age)
    {
        this.name = name;
        this.age = age;
    }

    public override string ToString()
    {
        return name + ": " + age;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{

    private Test script;

    void OnEnable()
    {
        script = (Test) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
        {
            script.RunTest();
        }
    }

}

#endif