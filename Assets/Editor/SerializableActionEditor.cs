using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(SerializableAction))]
public class SerializableActionEditor : PropertyDrawer
{

    private static Dictionary<Type, List<SerializableMethod>> typeToMethod = new Dictionary<Type, List<SerializableMethod>>();
    private static Dictionary<Type, string[]> typeToMethodNames = new Dictionary<Type, string[]>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 3f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.BeginChangeCheck();
        SerializableAction action = (SerializableAction) SerializedPropertyHelper.GetTargetObjectOfProperty(property);

        EditorGUI.LabelField(position, label);
        EditorGUI.indentLevel++;

        position.y += EditorGUIUtility.singleLineHeight;
        action.TargetObject = EditorGUI.ObjectField(position, "Target Object", action.TargetObject, typeof(Object), true);

        var type = action.TargetObject.GetType();
        EnsureMethodsCached(type);

        var methods = typeToMethod[type];
        var currentIdx = methods.IndexOf(action.TargetMethod);
        var nameIdx = currentIdx + 1;

        position.y += EditorGUIUtility.singleLineHeight;
        var newNameIdx = EditorGUI.Popup(position, "Target Method", nameIdx, typeToMethodNames[type]);

        if (newNameIdx != nameIdx)
        {
            if (newNameIdx == 0)
            {
                action.TargetMethod = null;
            }
            else
            {
                action.TargetMethod = methods[newNameIdx - 1];
            }
        }

        EditorGUI.indentLevel--;
        if (EditorGUI.EndChangeCheck())
        {
            var targetObject = property.serializedObject.targetObject;
            EditorUtility.SetDirty(targetObject);
            var asComp = targetObject as Component;
            if (asComp != null)
            {
                EditorSceneManager.MarkSceneDirty(asComp.gameObject.scene);
            }

        }
    }

    private static void EnsureMethodsCached(Type type)
    {
        if (!typeToMethod.ContainsKey(type))
        {
            var methods = MethodLister.SerializeableMethodsOn(type);
            var methodNames = new string[methods.Count + 1];
            methodNames[0] = "None selected";
            for (int i = 0; i < methods.Count; i++)
            {
                methodNames[i + 1] = methods[i].ToString();
            }

            typeToMethod[type] = methods;
            typeToMethodNames[type] = methodNames;
        }
    }
}
