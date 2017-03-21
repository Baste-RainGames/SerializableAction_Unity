using UnityEngine;

namespace SerializableActions.Internal
{
    public static class EditorUtil
    {
        public static Rect NextPosition(Rect currentPosition, float nextPositionHeight)
        {
            currentPosition.y += currentPosition.height + 1;
            currentPosition.height = nextPositionHeight;
            return currentPosition;
        }
    }
}