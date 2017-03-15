# SerializableAction_Unity
A faster and more flexible replacement for Unity's UnityEvent.

## Introduction

This project offers the SerializableAction type, which is a replacement for Unity's UnityEvent. Like UnityEvent, you can use SerializableAction to assign callbacks in the Unity inspector, and call them at runtime. There are two major differences:

- SerializableAction supports methods with any number of parameters, unlike UnityEvent, which only supports methods with 0 or 1 parameter
- SerializableAction is about twice as fast as UnityEvent.

## Usage

SerializableAction is designed to work exactly like UnityEvent. You declare a public (or [SerializeField] private) field of the SerializableAction type, assign method(s) to the field in the inspector, and invoke the event at runtime with the Invoke() method.

For example, if you want a script that does something interesting when the player walks into a trigger collider, that would look like this:

```c#
public class PlayerEnterAction : MonoBehaviour
{

    [SerializeField]
    private SerializableAction action;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            action.Invoke();
        }
    }
}
```

## Installation

Download the project and copy the Assets folder contents to your project.

This is a WIP version of the project, so some test files are included that will be removed soon. A .dll version will be made available pretty soon.


## Limitations

To be able to assign a method in the inspector, all of the parameters of that method needs to be of a type the framework can draw. This means that it has to have a EditorGUI drawer for the type. UnityEvent has the same drawback.

Ideally, Unity's PropertyField could be used to draw arbitrary serialized data. This requires a SerializedProperty, though, and it seems impossible to create those without knowing the property's type at compile time. If somebody finds a way to work around this drawback, that contribution would be very welcome, as it'd add a lot of flexibility and delete a bunch of code.

## Dependencies, Licenses

Fasterflect is used to achieve faster .Invoke calls than UntiyAction:
- Licence: Apache 2.0
- Available at: https://fasterflect.codeplex.com/

FullSerializer is used to serialize some types of arguments:
- License: MIT
- Available at: https://github.com/jacobdufault/fullserializer

In addition, the type serialization is based on code by Bryan Keiren:
- Code used: http://bryankeiren.com/files_public/UnityScripts/SerializableSystemType.cs
- Bryan Keiren: http://www.bryankeiren.com/
