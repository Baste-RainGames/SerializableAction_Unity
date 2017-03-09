using System.Collections.Generic;
using UnityEditor;
using UnityEngineInternal;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    private List<SerializableMethod> methods;
    private MethodListerOptions options;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        options = (MethodListerOptions) EditorGUILayout.EnumMaskField("Method lister options", options);
        methods = MethodLister.SerializeableMethodsOn(target.GetType(), options);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Found methods:");
        EditorGUI.indentLevel++;
        foreach (var method in methods)
        {
            EditorGUILayout.LabelField(method.ToString());
        }
        EditorGUI.indentLevel--;
    }
}

