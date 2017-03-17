using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace SerializableActions.Internal
{
    [CustomPropertyDrawer(typeof(SerializableAction))]
    public class SerializableActionDrawer : PropertyDrawer
    {

        private ReorderableList reorderable;
        // The cache is not used for optimization, but to work around problems where Unity
        // double-iterated the property, causing it to try to draw other properties instead of
        // the SerializableAction
        private Dictionary<int, float> cachedHeights = new Dictionary<int, float>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (reorderable == null)
                CreateReorderable(property, label);

            return reorderable.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (reorderable == null)
                CreateReorderable(property, label);

            reorderable.DoList(position);
            property.serializedObject.ApplyModifiedProperties();
        }

        private void CreateReorderable(SerializedProperty property, GUIContent label)
        {
            var actionsProp = property.FindPropertyRelative("actions");
            var defaults = new ReorderableList.Defaults();

            reorderable = new ReorderableList(property.serializedObject, actionsProp, false, true, true, true);
            reorderable.draggable = true;
            reorderable.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);
            reorderable.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2;
                rect.height -= 2;
                SerializableAction_SingleDrawer.DrawSerializableAction(
                    rect, actionsProp.GetArrayElementAtIndex(index)
                );
            };
            reorderable.elementHeightCallback = index =>
            {
                var height = SerializableAction_SingleDrawer.FindSerializableActionHeight(
                                 actionsProp.GetArrayElementAtIndex(index), GUIContent.none) + 4f;
                cachedHeights[index] = height;
                return height;
            };
            reorderable.showDefaultBackground = true;

            reorderable.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                var rectHeight = EditorGUIUtility.singleLineHeight;
                float height;
                if (cachedHeights.TryGetValue(index, out height))
                    rectHeight = height;
                rect.height = rectHeight;

                defaults.DrawElementBackground(rect, index, active, focused, true);
            };

            reorderable.onAddCallback = list =>
            {
                defaults.DoAddButton(list);
                //If this is the first element, set the call state to the correct default. Otherwise do what the reorderable always does, which is
                //copy from the element above
                if (list.index == 0)
                {
                    var addedObj = actionsProp.GetArrayElementAtIndex(list.index);
                    addedObj.FindPropertyRelative("callState").enumValueIndex = (int) UnityEventCallState.RuntimeOnly;
                }
            };
        }
    }
}