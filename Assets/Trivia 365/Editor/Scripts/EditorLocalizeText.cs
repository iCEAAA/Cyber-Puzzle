using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizeText))]
public class EditorLocalizeText : Editor
{
    private LocalizeText TargetScript;
    private LocalizationManager LocManager;

    private int Selected = 0;

    public void OnEnable()
    {
        TargetScript = (LocalizeText)target;
        LocManager = (LocalizationManager)FindObjectOfType(typeof(LocalizationManager));
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(5);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("Localization Key",TargetScript.Key, EditorStyles.textField);

        EditorGUI.BeginChangeCheck();
        Selected = EditorGUILayout.Popup(Selected, LocManager.LocalizationKeys.ToArray(), EditorStyles.radioButton, GUILayout.Width(20));
        if (EditorGUI.EndChangeCheck())
        {
            TargetScript.Key = LocManager.LocalizationKeys[Selected];
            EditorUtility.SetDirty(TargetScript);
            Selected = 0;
        }

        GUILayout.EndHorizontal();

        TargetScript.LocalizeOnStart = EditorGUILayout.Toggle("Localize On Start", TargetScript.LocalizeOnStart);

        GUILayout.Space(5);
    }
}