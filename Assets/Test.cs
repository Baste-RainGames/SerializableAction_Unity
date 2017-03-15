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
    public UnityEvent unityEvent;

    public int sum;

    public void Foo(Person p)
    {
        sum += p.Age + p.Name.Length;
    }

    public void Bar(int i)
    {
        for (int j = 0; j < 10; j++)
        {
            if (Random.Range(0, 10) != 0)
                sum += i;
            else
                sum -= j;
        }
    }

    public IEnumerator RunTest()
    {
        var numRuns = 100000;

        float beforeUnity = Time.realtimeSinceStartup;
        for (int i = 0; i < numRuns; i++)
        {
            unityEvent.Invoke();
        }

        float afterUnity = Time.realtimeSinceStartup;

        yield return new WaitForSeconds(.1f);

        float beforeCustom = Time.realtimeSinceStartup;
        for (int i = 0; i < numRuns; i++)
        {
            serializableAction.Invoke();
        }

        float afterCustom = Time.realtimeSinceStartup;

        yield return new WaitForSeconds(.1f);
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
            script.StartCoroutine(script.RunTest());
        }
    }

}

#endif