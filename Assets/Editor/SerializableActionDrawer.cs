using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SerializableActions.Internal {
    [CustomPropertyDrawer(typeof(SerializableAction))]
    public class SerializableActionDrawer : PropertyDrawer {

        private ReorderableList reorderable;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (reorderable == null)
                CreateReorderable(property, label);

            return reorderable.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (reorderable == null)
                CreateReorderable(property, label);

            reorderable.DoList(position);
        }

        private Dictionary<int, float> cachedHeights = new Dictionary<int, float>();

        private void CreateReorderable(SerializedProperty property, GUIContent label) {
            var actionsProp = property.FindPropertyRelative("actions");
            var defaults = new ReorderableList.Defaults();

            reorderable = new ReorderableList(property.serializedObject, actionsProp);
            reorderable.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);
            reorderable.drawElementCallback = (rect, index, active, focused) =>
                SerializableAction_SingleDrawer.DrawSerializableAction(
                    rect, actionsProp.GetArrayElementAtIndex(index)
                );
            reorderable.elementHeightCallback = index => {
                var height = SerializableAction_SingleDrawer.FindSerializableActionHeight(
                            actionsProp.GetArrayElementAtIndex(index), GUIContent.none) + 2f;
                cachedHeights[index] = height;
                return height;
            };
            reorderable.showDefaultBackground = true;

            reorderable.drawElementBackgroundCallback = (rect, index, active, focused) => {
                //Trying to call this causes Unity's editor to somehow keep iterating the parent property, and
                //GetArrayElementAtIndex will return the next property (so the property BELOW THE SerializableAction IN THE INSPECTOR)
                //So the value is cached instead
                /*rect.height = SerializableAction_SingleDrawer.FindSerializableActionHeight(
                                  actionsProp.GetArrayElementAtIndex(index), GUIContent.none) + 2f;*/

                var rectHeight = EditorGUIUtility.singleLineHeight;
                float height;
                if (cachedHeights.TryGetValue(index, out height))
                    rectHeight = height;
                rect.height = rectHeight;

                defaults.DrawElementBackground(rect, index, active, focused, true);
            };
        }
    }
}