using SerializableActions.Internal;

namespace SerializableActions
{
    /// <summary>
    /// This is a replacement for the built-in UnityAction. It supports any number of arguments, and is faster than
    /// UnityAction.
    /// </summary>
    [System.Serializable]
    public class SerializableAction
    {
        public SerializableAction_Single[] actions;

        public void Invoke()
        {
            foreach (var action in actions)
            {
                action.Invoke();
            }
        }
    }
}