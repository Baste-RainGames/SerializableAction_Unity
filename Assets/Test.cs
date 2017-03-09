using System;
using System.Reflection;
using UnityEngine;

public class Test : MonoBehaviour
{
    public SerializableAction serializableAction1;
    /*public SerializableAction serializableAction2;
    public SerializableAction serializableAction3;
    public SerializableAction serializableAction4;
    public SerializableAction serializableAction5;
    public SerializableAction serializableAction6;*/

    [ContextMenu("SetUp")]
    public void SetUp()
    {
        var serMet1 = new SerializableMethod(GetType().GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance));
        serializableAction1 = new SerializableAction(this, serMet1);

        /*var foo2Int = GetType().GetMethod("Foo2",
                                          BindingFlags.Public | BindingFlags.Instance,
                                          null,
                                          CallingConventions.Any,
                                          new Type[] {typeof(int),},
                                          null);

        var serMet2 = new SerializableMethod(foo2Int);
        serializableAction2 = new SerializableAction(this, serMet2, 15);

        var foo2Str = GetType().GetMethod("Foo2",
                                          BindingFlags.Public | BindingFlags.Instance,
                                          null,
                                          CallingConventions.Any,
                                          new Type[] {typeof(string),},
                                          null);

        var serMet3 = new SerializableMethod(foo2Str);
        serializableAction3 = new SerializableAction(this, serMet3, "hello world!");

        var foo3Person = GetType().GetMethod("Foo3").MakeGenericMethod(typeof(Person));
        var serMet4 = new SerializableMethod(foo3Person);
        serializableAction4 = new SerializableAction(this, serMet4, new Person("john", 123));

        var foo3bool = GetType().GetMethod("Foo3").MakeGenericMethod(typeof(bool));
        var serMet5 = new SerializableMethod(foo3bool);
        serializableAction5 = new SerializableAction(this, serMet5, true);

        var mixNonGen = GetType().GetMethod("MixGenAndOver",
                                            BindingFlags.Public | BindingFlags.Instance,
                                            null,
                                            CallingConventions.Any,
                                            new Type[] {typeof(int),},
                                            null);
        var setMet6 = new SerializableMethod(mixNonGen);
        serializableAction6 = new SerializableAction(this, setMet6, 22);*/
    }

    public void Foo()
    {
        Debug.Log("Foo!");
    }

    public void Foo2(int i)
    {
        Debug.Log("Foo2 int: " + i);
    }

    public void Foo2(string s)
    {
        Debug.Log("Foo2 string: " + s);
    }

    public void Foo3<T>(T t)
    {
        Debug.Log("Foo3, type is " + typeof(T) + ", value is: " + t);
    }

    public void PartialGen<T>(int i, T t)
    {
        Debug.Log("i: " + i + ", t: " + t);
    }

    public void MixGenAndOver(int i)
    {
        Debug.Log("non-generic, int is " + i);
    }

    public void MixGenAndOver<T>(T t)
    {
        Debug.Log("generic, value is " + t);
    }

    public string YAY(int i)
    {
        return "i as string: " + i;
    }

    private void Start()
    {
        /*Debug.Log("action 1:");
        serializableAction1.Invoke();
        Debug.Log("action 2:");
        serializableAction2.Invoke();
        Debug.Log("action 3:");
        serializableAction3.Invoke();
        Debug.Log("action 4:");
        serializableAction4.Invoke();
        Debug.Log("action 5:");
        serializableAction5.Invoke();
        Debug.Log("action 6:");
        serializableAction6.Invoke();*/
    }

}

[System.Serializable]
public class Person
{
    [SerializeField]
    private string name;
    [SerializeField]
    private int age;

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