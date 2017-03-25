# SerializableAction_Unity
A faster and more flexible replacement for Unity's UnityEvent.

## Introduction

This project offers the SerializableAction type, which is a replacement for Unity's UnityEvent. Like UnityEvent, you can use SerializableAction to assign callbacks in the Unity inspector, and call them at runtime. There are some major improvements over UnityEvent:

- Support for methods with any number of parameters. UnityEvent only supports methods with 0 or 1 parameters.
- Support for parameters with custom types (see Limitations). UnityEvent only supports a narrow range of predefined types.
- A SerializableAction is about twice as fast to Invoke as the same UnityEvent.

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


This allows you to assign methods in the inspector:

![readme example](https://github.com/Baste-RainGames/SerializableAction_Unity/blob/master/readme_example.png)

## Installation

Download the project and copy the Assets folder contents to your project.
A .dll version will (probably) be made available pretty soon.

## Limitations

Fields are not supported for now. So while you can use a property setter, you can't set a public field. This should be fixed pretty soon.

Generic methods are not supported. Supporting this is not hard from an implementation standpoint (it already works in the back-end), but figuring out how to draw a selector for the type parameters is harder. It will probably never be supported directly, but some other solution might show up (see Future plans)

## Future plans
- Look into creating a SerializableFunc<T>, which has a return type. Like SerializableAction, you can assign any method to it, but this will only allow methods with the return type T. Since Unity doesn't support serializing generic fields, this will probably require subclassing a specific SerializableFunc
- Look into support for Coroutine methods. It should be possible to assign StartCorotuine to a SerializableAction, but that would require creating a drop-down for which Coroutine to start, and also a way to define parameters to the coroutine.
- Look into generic SerializableActions, which restrict the assignable methods to ones that match the correct parameters. This would also allow for assigning generic methods if the generic restrictions match.
- Figure out a good way to override the default settings. SerializableAction filters out a bunch of methods I believe are not interesting, like editor-only properties (runInEditMode) and the whole Messaging (MonoBehaviour.SendMessage) and Invoke (MonoBehaviour.Invoke(Repeating)) systems. It might be interesting for developers to change what's getting filtered, though, so I'd like there to be a way to set these filters, both globally and on a per-action basis (probably through attributes).
- SerializableAction should have InvokeDelayed and InvokeRepeating functions. I'll have to look into how to handle the action getting changed between the call to eg. InvokeDelayed and when the delay actually happens. To make the delay work with Unity (with regards to UnityEngine stuff happening on the main thread), this (probably) needs to happen through a Coroutine, so I'll want to figure out how to find a MonoBehaviour to run that Coroutine on (Static coroutine? send a MB as a parameter?).

## Goals
SerializableAction should be simple. UnityEvent is a very powerfull tool, and this project seeks to overcome the problems UnityEvent have - primarilly how restrictive UnityEvent is concerning which parameters a method can have.

This means that I will not want to extend the project to support complex features like:
- Assigning the return value of methods as arguments to SerializedActions, or any kind of piping (bash' | operator) setup.
- Adding delays to when the method will get invoked in the inspector. 
- Having type selectors for generic methods in the inspector
- Supporting static methods

All of these things would make the inspector large and cumbersome. If someone wants support for these things and are willing to do the work, I'd be happy to take the pull request if the feature either is encapsulated in it's own class, or is unlocked by adding a parameter to the SerializableAction (ie. [SupportDelayedCalls])

## Contributions
I'll want to fix all of the things under Limitations that says "will be fixed", and do a proper cleanup/commenting pass before I start taking pull requests. This is to have a sensible, finished base to build from. I'll not really tell people about this before that's done, so if you read this before that, you're special I guess? Or you're reading through git logs ¯\_(ツ)_/¯

After that, I'll be happy to accept pull requests. If you can
- Remove one of the .dll dependencies
- Speed up Invoke

I'll be really happy.

I'd love to get feedback on the current state of things, bug reports, and suggestions for improving the interface. 

I'd also love to get feedback on platform support. The reflection stuff is happening on deserialization, so the code should work on eg. IOS, but it probably doesn't actually work on IOS. 

Finally, I've got a lot of experience with serialization libraries failing really, really hard on AOT platforms, especially on consoles. I'd love pull requests that makes this run on either of the major consoles (and minor ones too, I guess). If writing "this fixes the thing for console X" breaks NDA, some heavy nudge-nudge-wink-wink can probably get us around that. I'll want to do add console support eventually myself, but if somebody wants to fix it sooner than that, that'd be fantastic.

## Open source projects used:
There's quite a few open source projects that made creating this a lot easier. The ones currently used are:

## Fasterflect
Used to achieve faster .Invoke calls than UntiyAction:
- Licence: Apache 2.0
- Available at: https://fasterflect.codeplex.com/

## FullSerializer 
Used to handle serialization and deserialization of arguments:
- License: MIT
- Available at: https://github.com/jacobdufault/fullserializer

## SpacePuppy
Code for extracting the actual object from a ScriptableObject has been extracted from the SpacePuppy framework by Dylan Engelman:
- Available at: https://github.com/lordofduct/spacepuppy-unity-framework
- License: https://github.com/lordofduct/spacepuppy-unity-framework#license
- Specific code used is GetTargetObjectOfProperty and dependencies from: https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs

## SerializableSystemType
The type serialization is based on code by Bryan Keiren:
- Code used: http://bryankeiren.com/files_public/UnityScripts/SerializableSystemType.cs
- Bryan Keiren: http://www.bryankeiren.com/
