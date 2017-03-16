using System;
using UnityEngine;
using UnityEngine.Events;
using SerializableActions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour
{
    public UnityEvent unityAction;
    public SerializableAction serializableAction;
    public int sum;

    public void Foo(Person p)
    {
        sum += p.Age + p.Name.Length;
    }

    public void Bar(int i)
    { }

    public void Bar(Rect r)
    { }

    public void Bar(string s)
    { }

    public void Bar(float f, bool b)
    { }

    public void Bar(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10, int i11, int i12)
    { }

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