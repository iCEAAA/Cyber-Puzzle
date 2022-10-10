using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizApp.Editor;
using System;

[CustomEditor(typeof(LocalizationManager))]
public class EditorLocalizationManager : Editor
{
    private LocalizationManager TargetScript;

    private string LangName;
    private string KeyName;

    private bool ShowAddSection;

    private const string pattern = @"^[a-zA-Z0-9\_]+$";
    private Regex regex;

    private string tabPref = "curLocaleEditorTab";
    private int tab;

    private static readonly string[] PresetLanguages = { "", "Afrikaans", "Albanian", "Amharic", "Arabic", "Aramaic", "Armenian", "Assamese", "Aymara", "Azerbaijani", "Balochi", "Bamanankan", "Bashkort", "Basque", "Belarusan", "Bengali", "Bhojpuri", "Bislama", "Bosnian", "Brahui", "Bulgarian", "Burmese", "Cantonese", "Catalan", "Cebuano", "Chechen", "Cherokee", "Chinese", "Croatian", "Czech", "Dakota", "Danish", "Dari", "Dholuo", "Dutch English", "Esperanto", "Estonian", "Finnish", "French", "Georgian", "German", "Greek", "Guarani", "Gujarati", "Haitian Creole", "Hausa", "Hawaiian", "Hawaiian Creole", "Hebrew", "Hiligaynon", "Hindi", "Hungarian", "Icelandic", "Igbo", "Ilocano", "Indonesian", "Inuit", "Irish Gaelic", "Italian", "Japanese", "Jarai", "Javanese", "Kabyle", "Kannada", "Kashmiri", "Kazakh", "Khmer", "Khoekhoe Korean", "Kurdish", "Kyrgyz", "Lao", "Latin", "Latvian", "Lingala", "Lithuanian", "Macedonian", "Maithili", "Malagasy", "Malay", "Malayalam", "Mandarin", "Marathi", "Mende", "Mongolian", "Nahuatl", "Navajo", "Nepali", "Norwegian", "Ojibwa", "Oriya", "Oromo", "Pashto", "Persian", "Polish", "Portuguese", "Punjabi", "Quechua", "Romani", "Romanian", "Russian", "Rwanda", "Samoan", "Sanskrit", "Serbian Shona", "Sindhi", "Sinhala", "Slovak", "Slovene", "Somali", "Spanish", "Swahili", "Swedish", "Tachelhit", "Tagalog", "Tajiki", "Tamil", "Tatar", "Telugu", "Thai", "Tibetic", "Tigrigna", "Tok Pisin", "Turkish", "Turkmen", "Ukrainian", "Urdu", "Uyghur", "Uzbek", "Vietnamese", "Warlpiri", "Welsh", "Wolof", "Xhosa", "Yakut", "Yiddish", "Yoruba", "Yucatec", "Zapotec" };
    private int Selected;

    private bool ShouldSave;

    private bool ShowWarning;

    private string path;

    private void OnEnable()
    {
        TargetScript = (LocalizationManager)target;
        regex = new Regex(pattern);

        tab = EditorPrefs.GetInt(tabPref, 0);

        CheckSaveState();

        TargetScript.DisableImportButtons = false;
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(TargetScript, "");

        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        tab = GUILayout.Toolbar(tab, new string[] { "Languages", "Import", "Settings" }, CustomStyles.m_xlTabs);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt(tabPref, tab);
            CheckSaveState();
        }

        GUILayout.Space(10);

        if (tab == 0)
            DrawLanguagesTab();
        else if (tab == 1)
            DrawGeneratorTab();
        else
            DrawSettingsTab();

        GUILayout.Space(10);
    }

    private void DrawLanguagesTab()
    {
        ShowWarning = EditorGUILayout.Foldout(ShowWarning, "Warning", true);

        if (ShowWarning)
            EditorGUILayout.HelpBox("Changing the order WILL RESET the category languages. Kindly figure out the order you would like to use then stick to it.\nThis only applies to changing the order, adding a new language DOES NOT break anything.", MessageType.Warning);

        GUILayout.Space(5);

        if (ShouldSave)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("<b>Changes made. Save your work.</b>", CustomStyles.m_centeredHelpBox);
            GUI.color = Color.white;

            GUILayout.Space(5);
        }

        for (int x = 0; x < TargetScript.Languages.Count; x++)
        {
            GUILayout.BeginHorizontal(CustomStyles.m_toolbarBtn, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField(x == 0 ? string.Concat(TargetScript.Languages[x], " (Default)") : TargetScript.Languages[x], EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(x == 0);
            if (GUILayout.Button("▲", CustomStyles.m_toolbarBtn, GUILayout.Width(30)))
            {
                string tmp = TargetScript.Languages[x];
                TargetScript.Languages.RemoveAt(x);
                TargetScript.Languages.Insert(x - 1, tmp);

                GUIUtility.keyboardControl = 0;

                ShowWarning = true;

                CheckSaveState();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(x == TargetScript.Languages.Count - 1);
            if (GUILayout.Button("▼", CustomStyles.m_toolbarBtn, GUILayout.Width(30)))
            {
                string tmp = TargetScript.Languages[x];
                TargetScript.Languages.RemoveAt(x);
                TargetScript.Languages.Insert(x + 1, tmp);

                GUIUtility.keyboardControl = 0;

                ShowWarning = true;

                CheckSaveState();
            }
            EditorGUI.EndDisabledGroup();

            GUI.color = Color.red;

            if (GUILayout.Button("Delete", CustomStyles.m_toolbarBtn, GUILayout.Width(60)))
            {
                EditorApplication.Beep();

                if (TargetScript.Languages.Count == 1)
                {
                    EditorUtility.DisplayDialog("Localization Manager Error", "You need to have at least one language on the Localization Manager.", "OK");
                    return;
                }

                if (EditorUtility.DisplayDialog("Delete Language", string.Concat("Are you sure you want to delete ", TargetScript.Languages[x], " from the Localization Manager?"), "Yes", "No"))
                {
                    GUIUtility.keyboardControl = 0;

                    AssetDatabase.DeleteAsset(new string[] { "Assets", "Trivia 365", "Resources", "i18n", TargetScript.Languages[x] + ".json" }.Aggregate((a, b) => Path.Combine(a, b)));

                    TargetScript.Languages.RemoveAt(x);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                    CheckSaveState();

                    AssetDatabase.Refresh();
                }

            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        GUILayout.Space(5);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        GUI.color = Color.cyan;

        ShowAddSection = GUILayout.Toggle(ShowAddSection, "Add A New language", CustomStyles.m_standardBtn);

        GUI.color = Color.yellow;

        if (GUILayout.Button("Save Languages", CustomStyles.m_standardBtn))
            SaveLanguages();

        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (ShowAddSection)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUI.BeginChangeCheck();
            Selected = EditorGUILayout.Popup(Selected, PresetLanguages, EditorStyles.radioButton, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                LangName = PresetLanguages[Selected].Replace(" ", "");
                Selected = 0;
            }

            LangName = EditorGUILayout.TextField(LangName, CustomStyles.m_textField);

            GUI.color = Color.cyan;

            if (GUILayout.Button("Add Language", CustomStyles.m_xlToolbarBtn, GUILayout.Width(120)))
            {
                if (string.IsNullOrEmpty(LangName))
                {
                    EditorUtility.DisplayDialog("Localization Manager Error", "The language name cannot be empty or null.", "OK");
                    return;
                }

                if (LangName.Contains(" "))
                {
                    EditorUtility.DisplayDialog("Localization Manager Error", "The language name cannot contain any spaces.", "OK");
                    return;
                }

                if (char.IsNumber(LangName[0]))
                {
                    EditorUtility.DisplayDialog("Localization Manager Error", "The first letter in the language name cannot be a number.", "OK");
                    return;
                }

                if (!regex.IsMatch(LangName))
                {
                    EditorUtility.DisplayDialog("Localization Manager Error", "The language name contains invalid characters.\n\nAllowed Characters: a-z, A-Z, 0-9 or underscore.", "OK");
                    return;
                }

                foreach (string str in TargetScript.Languages)
                {
                    if (str.Equals(LangName))
                    {
                        EditorUtility.DisplayDialog("Localization Manager Error", string.Concat(LangName, " already exists on the Localization Manager."), "OK");
                        return;
                    }
                }

                TargetScript.Languages.Add(LangName);

                ClearLanguageField();

                EditorUtility.SetDirty(TargetScript);
            }

            GUI.color = Color.yellow;

            if (GUILayout.Button("Clear", CustomStyles.m_xlToolbarBtn, GUILayout.Width(75)))
            {
                ClearLanguageField();
            }

            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }
        else
        {
            GUI.color = Color.magenta;

            if (GUILayout.Button("Generate CSV Template", CustomStyles.m_standardBtn))
                TargetScript.ExportCSV();

            GUI.color = Color.white;
        }
    }

    private void SaveLanguages()
    {
        ShowAddSection = false;

        string[] res = Directory.GetFiles(Application.dataPath, "LocalizationManager.cs", SearchOption.AllDirectories);
        string saveLocation = res[0].Replace("\\LocalizationManager.cs", "").Replace("\\", "/");

        string fileBody = string.Empty;

        fileBody = string.Concat("//This file is auto-generated from the editor. DO NOT MODIFY.", NewLine(2), "using System.Collections.Generic;", NewLine(2), "public enum LanguageList", NewLine() + "{" + NewLine());

        string last = TargetScript.Languages.Last();

        foreach (string str in TargetScript.Languages)
        {
            if (str.Equals(last))
                fileBody += string.Concat("\t", str, NewLine());
            else
                fileBody += string.Concat("\t", str, ",", NewLine());
        }

        fileBody.Trim();

        fileBody += "}";

        fileBody += NewLine(2);

        fileBody += string.Concat("[System.Serializable]", NewLine(), "public class LocaleFormat", NewLine(), "{", NewLine(), "\tpublic string Key;", NewLine(), "\tpublic List<string> Languages = new List<string>();", NewLine(2));

        fileBody += string.Concat("\tpublic static LocaleFormat FromCsv(string Line)", NewLine(), "\t{", NewLine());

        fileBody += string.Concat("\t\tstring[] values = Line.Split(',');", NewLine(), "\t\tLocaleFormat value = new LocaleFormat();", NewLine(), "\t\tvalue.Key = values[0];", NewLine(2), "\t\tfor (int x = 1; x < values.Length; x++)", NewLine(), "\t\t\tvalue.Languages.Add(values[x]);", NewLine(2), "\t\treturn value;", NewLine());

        fileBody.Trim();

        fileBody += "\t}" + NewLine();

        fileBody.Trim();

        fileBody += "}";

        fileBody += NewLine(2);

        fileBody += string.Concat("public struct LocaleContainer", NewLine(), "{", NewLine(), "\tpublic List<LocaleFormat> Locale;", NewLine(2), "\tpublic LocaleContainer(List<LocaleFormat> data) { Locale = data; }", NewLine());

        fileBody.Trim();

        fileBody += "}";

        string path = Path.Combine(saveLocation, "Gen.cs");

        using (var wr = new StreamWriter(path, false))
        {
            wr.Write(fileBody);
            wr.Close();
        }

        AssetDatabase.Refresh();

        CheckSaveState();
    }

    private void ClearLanguageField()
    {
        CheckSaveState();

        LangName = string.Empty;
        GUIUtility.keyboardControl = 0;
    }

    private void DrawGeneratorTab()
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField("<b><size=13>Import Localization File</size></b>", CustomStyles.m_centerBoldLabel, GUILayout.ExpandWidth(true));

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(TargetScript.DisableImportButtons);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        GUI.color = Color.cyan;

        if (GUILayout.Button("Import from Local File", CustomStyles.m_standardBtn))
            TargetScript.ImportCSV();

        GUI.color = Color.yellow;

        if (GUILayout.Button("Import from Google Sheets", CustomStyles.m_standardBtn))
            TargetScript.StartDownload();

        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();
    }

    private void DrawSettingsTab()
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField("<b><size=13>Google Sheets Settings</size></b>", CustomStyles.m_centerBoldLabel, GUILayout.ExpandWidth(true));

        GUILayout.Space(10);

        GUILayout.Label("https://docs.google.com/spreadsheets/d/<b>1lWPF95B_Cr2IXYRo-20BdZrw6</b>/edit#gid=<b>2885456</b>", CustomStyles.m_centeredHelpBox);

        GUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        TargetScript.GoogleSheetID = EditorGUILayout.TextField("Google Sheets ID", TargetScript.GoogleSheetID);
        TargetScript.GoogleSheetGid = EditorGUILayout.TextField("Google Sheets GID", TargetScript.GoogleSheetGid);
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(TargetScript);

        GUILayout.Space(5);

        GUI.color = Color.magenta;

        if (!string.IsNullOrEmpty(TargetScript.GoogleSheetID))
        {
            if (GUILayout.Button("Open Spreadsheet", CustomStyles.m_standardBtn))
                Application.OpenURL("https://docs.google.com/spreadsheets/d/" + TargetScript.GoogleSheetID + "/edit?usp=sharing");
        }

        GUI.color = Color.white;

        GUILayout.Space(15);

        EditorGUILayout.LabelField("<b><size=13>Import Settings</size></b>", CustomStyles.m_centerBoldLabel, GUILayout.ExpandWidth(true));

        GUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        TargetScript.MinifyJson = EditorGUILayout.Toggle(new GUIContent("Minify JSON"), TargetScript.MinifyJson);
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(TargetScript);
    }

    private void CheckSaveState()
    {
        List<string> enumList = Enum.GetValues(typeof(LanguageList)).Cast<LanguageList>().Select(v => v.ToString()).ToList();

        if (enumList.Count != TargetScript.Languages.Count)
        {
            ShouldSave = true;
            return;
        }

        for (int x = 0; x < enumList.Count; x++)
        {
            if (!enumList[x].Equals(TargetScript.Languages[x]))
            {
                ShouldSave = true;
                return;
            }
        }

        ShouldSave = false;
    }

    private string NewLine(int Count = 1)
    {
        return string.Concat(Enumerable.Repeat(Environment.NewLine, Count).ToArray());
    }
}