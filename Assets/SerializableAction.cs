using Object = UnityEngine.Object;

[System.Serializable]
public class SerializableAction
{

    /// <summary>
    /// Object containing the event
    /// </summary>
    public Object targetObject;

    /// <summary>
    /// Method to call
    /// </summary>
    public SerializableMethod targetMethod;

    /// <summary>
    /// Invokes the action
    /// </summary>
    public void Invoke()
    {
        targetMethod.MethodInfo.Invoke(targetObject, null);
    }
}