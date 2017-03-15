using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableActionList))]
public class SerializableActionListDrawer : PropertyDrawer
{

    private ReorderableList reorderable;

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
    }

    private void CreateReorderable(SerializedProperty property, GUIContent label)
    {
        var actionsProp = property.FindPropertyRelative("actions");
        var defaults = new ReorderableList.Defaults();

        reorderable = new ReorderableList(property.serializedObject, actionsProp);
        reorderable.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label);
        reorderable.drawElementCallback = (rect, index, active, focused) =>
            SerializableActionDrawer.DrawSerializableAction(
                rect, actionsProp.GetArrayElementAtIndex(index)
            );
        reorderable.elementHeightCallback = index =>
            SerializableActionDrawer.FindSerializableActionHeight(
                actionsProp.GetArrayElementAtIndex(index), GUIContent.none) + 2f;
        reorderable.showDefaultBackground = true;

        reorderable.drawElementBackgroundCallback = (rect, index, active, focused) =>
        {
            rect.height = reorderable.elementHeightCallback(index);
            defaults.DrawElementBackground(rect, index, active, focused, true);
        };

    }
}