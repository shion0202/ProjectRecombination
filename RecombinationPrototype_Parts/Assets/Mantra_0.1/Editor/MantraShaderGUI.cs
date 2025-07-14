using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class MantraShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var baseColor = FindProperty("_BaseColor", properties);
        materialEditor.ShaderProperty(baseColor, "Main Color");

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Mantra Toon Shader 설정입니다.", MessageType.Info);
    }
}
