using UnityEngine;
using UnityEditor;
using System.IO;

public static class MaterialPresetCreator
{
    // [MenuItem("Tools/Mantra/Create ToonBase Presets")]
    // public static void CreateToonPresets()
    // {
    //     string shaderPath = "Shader Graphs/Toon/SG_ToonBase";
    //     Shader shader = Shader.Find($"Shader Graphs/{shaderPath}");
    //
    //     if (shader == null)
    //     {
    //         Debug.LogError("Shader not found!");
    //         return;
    //     }
    //
    //     string presetPath = "./Presets/";
    //     Directory.CreateDirectory(presetPath);
    //
    //     CreatePreset("Red", Color.red, shader, presetPath);
    //     CreatePreset("Blue", Color.blue, shader, presetPath);
    //     CreatePreset("Green", Color.green, shader, presetPath);
    // }

    // private static void CreatePreset(string name, Color color, Shader shader, string path)
    // {
    //     Material mat = new Material(shader);
    //     mat.name = $"M_ToonBase_{name}";
    //     mat.SetColor("_BaseColor", color);
    //     AssetDatabase.CreateAsset(mat, $"{path}{mat.name}.mat");
    // }
}