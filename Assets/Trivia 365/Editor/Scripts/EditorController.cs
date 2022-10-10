using QuizApp.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Controller))]
public class EditorController : Editor
{
    private const string DotweenKey = "USE_DOTWEEN";
    private static readonly char[] Seperators = new char[] { ';', ',', ' ' };

    public void OnEnable()
    {
        if (!AssetDatabase.IsValidFolder(Path.Combine("Assets", "Demigiant")))
            ToggleDefine(false);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

#if USE_DOTWEEN
        GUI.color = Color.red;

        if (GUILayout.Button("Disable UI Animations", CustomStyles.m_standardBtn))
            ToggleDefine(false);
#else
        GUI.color = Color.green;

        if (GUILayout.Button("Enable UI Animations", CustomStyles.m_standardBtn))
            ToggleDefine();
#endif
        GUI.color = Color.white;
        GUILayout.Space(10);
    }

    private static void ToggleDefine(bool Add = true)
    {
        var curTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var curDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(curTarget);

        if (Add)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine("Assets", "Demigiant")))
            {
                if (EditorUtility.DisplayDialog("Error", "We could not find the DOTween folder in your project.", "Get DOTween", "Close"))
                    UnityEditorInternal.AssetStore.Open("content/27676");

                return;
            }

            if (!curDefineSymbols.Contains(DotweenKey))
            {
                string[] DefineSymbols = curDefineSymbols.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
                List<string> newDefineSymbols = new List<string>(DefineSymbols);
                newDefineSymbols.Add(DotweenKey);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(curTarget, string.Join(";", newDefineSymbols.ToArray()));
            }
        }
        else
        {
            if (curDefineSymbols.Contains(DotweenKey))
            {
                string[] DefineSymbols = curDefineSymbols.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
                List<string> newDefineSymbols = new List<string>(DefineSymbols);
                newDefineSymbols.Remove(DotweenKey);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(curTarget, string.Join(";", newDefineSymbols.ToArray()));
            }
        }
    }
}
