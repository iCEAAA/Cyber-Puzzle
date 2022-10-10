using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizApp.Editor;

[CustomEditor(typeof(CategoryCreator))]
public class EditorCategoryCreator : Editor
{
    private CategoryCreator TargetScript;
    private LocalizationManager LocManager;
    private Controller Controller;
#if USE_ADMOB
    private AdmobManager AdmobManager;
#endif

    private int ActiveTab;
    private string TabPref = "CurCatCreatorTab";

    private bool ShowAllLanguages;
    private string ShowAllPref = "ShowAllState";
    private string CurLanguagePref = "CurSelectedLang";

    private LanguageList CurrentLang;

    private void OnEnable()
    {
        TargetScript = (CategoryCreator)target;

        LocManager = (LocalizationManager)FindObjectOfType(typeof(LocalizationManager));
        Controller = (Controller)FindObjectOfType(typeof(Controller));
#if USE_ADMOB
        AdmobManager = (AdmobManager)FindObjectOfType(typeof(AdmobManager));
#endif
        ActiveTab = EditorPrefs.GetInt(TabPref, 0);
        ShowAllLanguages = EditorPrefs.GetBool(ShowAllPref, true);
        CurrentLang = (LanguageList)EditorPrefs.GetInt(CurLanguagePref, 0);

        if ((int)CurrentLang >= LocManager.Languages.Count)
            CurrentLang = 0;
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(TargetScript, "");

        if (EditorApplication.isPlaying)
        {
            GUILayout.Space(25);
            EditorGUILayout.LabelField("You are in play mode.", CustomStyles.m_centerBoldLabel);
            GUILayout.Space(25);

            return;
        }

        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        ActiveTab = GUILayout.Toolbar(ActiveTab, new string[] { "Categories", "Settings", "Other" }, CustomStyles.m_xlTabs);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetInt(TabPref, ActiveTab);

        GUILayout.Space(13);

        switch (ActiveTab)
        {
            case 0:
                DrawCategoriesTab();
                break;

            case 1:
                DrawSettingsTab();
                break;

            case 2:
                DrawOtherTab();
                break;
        }

        GUILayout.Space(10);
    }

    private void DrawCategoriesTab()
    {
        bool CategoriesEmpty = TargetScript.CategoryList.Count > 0;

        GUILayout.BeginHorizontal();

        GUI.color = Color.cyan;

        if (GUILayout.Button("Add A New Category", CustomStyles.m_standardBtn))
        {
            if (ShowAllLanguages)
                TargetScript.CategoryList.Add(new CategoryFormat());
            else
                TargetScript.CategoryList.Add(new CategoryFormat(CurrentLang));
        }

        GUI.color = Color.yellow;

        EditorGUI.BeginDisabledGroup(!CategoriesEmpty);
        if (GUILayout.Button("Update Categories", CustomStyles.m_standardBtn))
            TargetScript.UpdateCategories();
        EditorGUI.EndDisabledGroup();

        GUI.color = Color.magenta;

        if (GUILayout.Button("Open Question Editor", CustomStyles.m_standardBtn))
            Application.OpenURL("https://mintonne.com/quizapp/editor/");

        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(CustomStyles.m_xlToolbarBtn, GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField(ShowAllLanguages ? "Language Selection - All Languages" : string.Concat("Language Selection - ", CurrentLang.ToString()), CustomStyles.m_boldLabel);

        ShowAllLanguages = GUILayout.Toggle(ShowAllLanguages, "ALL", CustomStyles.m_xlToolbarBtn, GUILayout.Width(50));

        EditorPrefs.SetBool(ShowAllPref, ShowAllLanguages);

        EditorGUI.BeginDisabledGroup(ShowAllLanguages);

        EditorGUI.BeginChangeCheck();
        CurrentLang = (LanguageList)EditorGUILayout.EnumPopup("", CurrentLang, CustomStyles.m_toolbarDropdown, GUILayout.Width(150));
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetInt(CurLanguagePref, (int)CurrentLang);

        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        for (int i = 0; i < TargetScript.CategoryList.Count; i++)
        {
            GUI.skin.textField.wordWrap = false;

            CategoryFormat format = TargetScript.CategoryList[i];

            if (!ShowAllLanguages && format.Language != CurrentLang)
                continue;

            GUILayout.BeginHorizontal(CustomStyles.m_toolbarBtn, GUILayout.ExpandWidth(true));

            format.Enabled = GUILayout.Toggle(format.Enabled, string.Empty, GUILayout.Width(20));

            EditorGUILayout.LabelField(string.Concat((i + 1).ToString(), ". ", format.CategoryName.ToString(), " (" + format.Mode.ToString(), " Mode)"), EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("▲", CustomStyles.m_toolbarBtn, GUILayout.Width(30)))
            {
                CategoryFormat tmp = TargetScript.CategoryList[i];
                TargetScript.CategoryList.RemoveAt(i);
                TargetScript.CategoryList.Insert(i - 1, tmp);

                GUIUtility.keyboardControl = 0;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(i == TargetScript.CategoryList.Count - 1);
            if (GUILayout.Button("▼", CustomStyles.m_toolbarBtn, GUILayout.Width(30)))
            {
                CategoryFormat tmp = TargetScript.CategoryList[i];
                TargetScript.CategoryList.RemoveAt(i);
                TargetScript.CategoryList.Insert(i + 1, tmp);

                GUIUtility.keyboardControl = 0;
            }
            EditorGUI.EndDisabledGroup();

            GUI.color = Color.yellow;

            if (GUILayout.Button("Clone", CustomStyles.m_toolbarBtn, GUILayout.Width(60)))
                TargetScript.CategoryList.Add(new CategoryFormat(format));

            GUI.color = Color.red;

            if (GUILayout.Button("Delete", CustomStyles.m_toolbarBtn, GUILayout.Width(60)))
            {
                EditorApplication.Beep();
                if (EditorUtility.DisplayDialog("Delete Category", string.Concat("Are you sure you want to delete the ", format.CategoryName.ToString(), " category?"), "Yes", "No"))
                {
                    GUIUtility.keyboardControl = 0;
                    TargetScript.CategoryList.RemoveAt(i);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (format.Enabled)
            {
                GUILayout.Space(-1);

                GUILayout.BeginVertical(CustomStyles.m_darkBackground());

                format.CategoryName = EditorGUILayout.TextField("Category Name", format.CategoryName);
                format.CategoryImage = (Sprite)EditorGUILayout.ObjectField("Category Image", format.CategoryImage, typeof(Sprite), false, GUILayout.Height(16));
                format.Mode = (DownloadMode)EditorGUILayout.EnumPopup(new GUIContent("Download Mode", "Where should we get the JSON file for this category."), format.Mode);

                if (format.Mode == DownloadMode.Online)
                {
                    GUI.color = Color.red;
                    if (format.OnlinePath.Length < 1)
                        EditorGUILayout.LabelField("Enter the <b>DIRECT</b> download link in the field below.", CustomStyles.m_helpBox);
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("Download link set.", CustomStyles.m_helpBox);
                    }
                    GUI.color = Color.white;

                    GUILayout.BeginHorizontal();

                    format.OnlinePath = EditorGUILayout.TextField("Online Path", format.OnlinePath);

                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        GUIUtility.keyboardControl = 0;
                        TargetScript.OnlineTest(format.CategoryName, format.OnlinePath);
                    }

                    GUILayout.EndHorizontal();
                }
                else if (format.Mode == DownloadMode.Offline)
                {
                    GUI.color = Color.red;
                    if (format.OfflineFile == null)
                        EditorGUILayout.LabelField("Drag & drop the question <b>JSON</b> file into the field below.", CustomStyles.m_helpBox);
                    else if(!TargetScript.TestFilename(format.OfflineFile.name))
                        EditorGUILayout.LabelField("Invalid filename. Allowed charcters: Aa-Zz and 0-9", CustomStyles.m_helpBox);
                    else
                    {
                        GUI.color = Color.green;

                        EditorGUILayout.LabelField("Offline file set.", CustomStyles.m_helpBox);
                    }
                    GUI.color = Color.white;

                    GUILayout.BeginHorizontal();

                    format.OfflineFile = (TextAsset)EditorGUILayout.ObjectField("Offline File", format.OfflineFile, typeof(TextAsset), false);

                    if (format.OfflineFile != null && !Path.GetExtension(AssetDatabase.GetAssetPath(format.OfflineFile)).ToLower().Equals(".json"))
                    {
                        format.OfflineFile = null;
                        EditorUtility.DisplayDialog("Error", "The selected file is not a JSON file.", "OK");
                    }

                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        GUIUtility.keyboardControl = 0;
                        TargetScript.OfflineTest(format.CategoryName, format.OfflineFile.text);
                    }

                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUI.color = Color.red;
                    if (format.OnlinePath.Length < 1)
                        EditorGUILayout.LabelField("Enter the <b>DIRECT</b> download link in the field below.", CustomStyles.m_helpBox);
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("Download link set.", CustomStyles.m_helpBox);
                    }
                    GUI.color = Color.white;

                    GUILayout.BeginHorizontal();

                    format.OnlinePath = EditorGUILayout.TextField(new GUIContent("Online Path"), format.OnlinePath);

                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        GUIUtility.keyboardControl = 0;
                        TargetScript.OnlineTest(format.CategoryName, format.OnlinePath);
                    }

                    GUILayout.EndHorizontal();

                    GUI.color = Color.red;
                    if (format.OfflineFile == null)
                        EditorGUILayout.LabelField("Drag & drop the question <b>JSON</b> file into the field below.", CustomStyles.m_helpBox);
                    else
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("Offline file set.", CustomStyles.m_helpBox);
                    }
                    GUI.color = Color.white;

                    GUILayout.BeginHorizontal();

                    format.OfflineFile = (TextAsset)EditorGUILayout.ObjectField("Offline File", format.OfflineFile, typeof(TextAsset), false);

                    if (format.OfflineFile != null && !Path.GetExtension(AssetDatabase.GetAssetPath(format.OfflineFile)).ToLower().Equals(".json"))
                    {
                        format.OfflineFile = null;
                        EditorUtility.DisplayDialog("Error", "The selected file is not a JSON file.", "OK");
                    }

                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        GUIUtility.keyboardControl = 0;
                        TargetScript.OfflineTest(format.CategoryName, format.OfflineFile.text);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(2);

                format.Language = (LanguageList)EditorGUILayout.EnumPopup("Language", format.Language);

                if ((int)format.Language >= LocManager.Languages.Count)
                    format.Language = 0;

                format.HighscorePref = EditorGUILayout.TextField(new GUIContent("Highscore PlayerPref", "A unique ID used to store the highscore for this category."), format.HighscorePref);

                if (GUILayout.Button("Autofill PlayerPref"))
                {
                    string Catname = format.CategoryName.Replace(" ", string.Empty);

                    if (Catname.Length > 20)
                        Catname.Substring(0, 20);

                    string NewPrefName = Catname.ToString() + "ScorePref";

                    if (string.IsNullOrEmpty(format.HighscorePref))
                        format.HighscorePref = NewPrefName;

                    if (!string.Equals(format.HighscorePref, NewPrefName))
                    {
                        if (EditorUtility.DisplayDialog("PlayerPref Warning", "All previous saved progress will be lost if the Highscore PlayerPref name is changed. DO NOT change it if you have published your app.\nAre you sure you want to change it?", "Yes", "Cancel"))
                            format.HighscorePref = NewPrefName;
                    }
                }

                GUILayout.Space(5);

                format.ShuffleQuestions = EditorGUILayout.Toggle(new GUIContent("Shuffle Questions", "Should we shuffle the questions in this category?"), format.ShuffleQuestions);

                format.ShuffleAnswers = EditorGUILayout.Toggle(new GUIContent("Shuffle Answers", "Should we shuffle the answers in this category?"), format.ShuffleAnswers);

                format.LimitQuestions = EditorGUILayout.Toggle(new GUIContent("Limit Questions", "Should we limit the number of questions per game for this category?"), format.LimitQuestions);

                if (format.LimitQuestions)
                {
                    format.QuestionLimit = EditorGUILayout.IntSlider("Question Limit", format.QuestionLimit, 1, 100);
                }

                format.CustomTimerAmount = EditorGUILayout.Toggle(new GUIContent("Custom Timer Amount", "Should we ignore the universal timer for this category?"), format.CustomTimerAmount);

                if (format.CustomTimerAmount)
                {
                    format.TimerAmount = EditorGUILayout.IntSlider("Timer Amount", format.TimerAmount, 5, 100);
                }

                format.CustomLivesAmount = EditorGUILayout.Toggle(new GUIContent("Custom Lives Amount", "Should we ignore the universal lives count for this category?"), format.CustomLivesAmount);

                if (format.CustomLivesAmount)
                {
                    format.LivesCount = EditorGUILayout.IntSlider("Lives Count", format.LivesCount, 1, 100);
                }

                GUILayout.Space(5);

                GUILayout.EndVertical();
            }

            GUILayout.Space(5);
        }

        EditorUtility.SetDirty(TargetScript);
    }

    private void DrawSettingsTab()
    {
        #region Category Page Settings

        GUILayout.BeginHorizontal(CustomStyles.m_xlToolbarBtn, GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("Category Page Settings", CustomStyles.m_boldLabel);

        if (GUILayout.Button("Home", CustomStyles.m_xlToolbarBtn, GUILayout.Width(70)))
            ShowHome();
        if (GUILayout.Button("Categories", CustomStyles.m_xlToolbarBtn, GUILayout.Width(90)))
            ShowCategories();

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        TargetScript.CategoryImageSize = EditorGUILayout.IntSlider("Image Size", TargetScript.CategoryImageSize, 50, 300);
        TargetScript.CategoryColumnCount = EditorGUILayout.IntSlider("Number of Columns", TargetScript.CategoryColumnCount, 2, 3);

        GUILayout.Space(10);

        TargetScript.CategoryFont = (Font)EditorGUILayout.ObjectField("Text Font", TargetScript.CategoryFont, typeof(Font), true, GUILayout.Height(16));
        TargetScript.CategoryFontStyle = (FontStyle)EditorGUILayout.EnumFlagsField("Text FontStyle", TargetScript.CategoryFontStyle);
        TargetScript.CategoryBestFit = EditorGUILayout.Toggle("Resize Text For Best Fit", TargetScript.CategoryBestFit);

        if (TargetScript.CategoryBestFit)
        {
            TargetScript.CategoryMinSize = EditorGUILayout.IntSlider("Minimum Font Size", TargetScript.CategoryMinSize, 20, 100);
            TargetScript.CategoryMaxSize = EditorGUILayout.IntSlider("Maximum Font Size", TargetScript.CategoryMaxSize, 20, 100);
        }
        else
        {
            TargetScript.CategoryFontSize = EditorGUILayout.IntSlider("Font Size", TargetScript.CategoryFontSize, 20, 100);
        }

        #endregion

        #region Highscores Page Settings

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(CustomStyles.m_xlToolbarBtn, GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("Profile Page Settings", CustomStyles.m_boldLabel);

        if (GUILayout.Button("Home", CustomStyles.m_xlToolbarBtn, GUILayout.Width(70)))
            ShowHome();
        if (GUILayout.Button("Highscores", CustomStyles.m_xlToolbarBtn, GUILayout.Width(90)))
            ShowHighscores();

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        TargetScript.ProfileImageSize = EditorGUILayout.IntSlider("Image Size", TargetScript.ProfileImageSize, 50, 300);
        TargetScript.ProfileColumnCount = EditorGUILayout.IntSlider("Column Count", TargetScript.ProfileColumnCount, 2, 3);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Profile Name Settings", CustomStyles.m_centerBoldLabel);

        GUILayout.Space(10);

        TargetScript.ProfileNameFont = (Font)EditorGUILayout.ObjectField("Category Name Font", TargetScript.ProfileNameFont, typeof(Font), true, GUILayout.Height(16));
        TargetScript.ProfileNameFontStyle = (FontStyle)EditorGUILayout.EnumFlagsField("Category Name FontStyle", TargetScript.ProfileNameFontStyle);
        TargetScript.ProfileNameBestFit = EditorGUILayout.Toggle("Resize Text For Best Fit", TargetScript.ProfileNameBestFit);

        if (TargetScript.ProfileNameBestFit)
        {
            TargetScript.ProfileNameMinSize = EditorGUILayout.IntSlider("Minimum Font Size", TargetScript.ProfileNameMinSize, 20, 100);
            TargetScript.ProfileNameMaxSize = EditorGUILayout.IntSlider("Maximum Font Size", TargetScript.ProfileNameMaxSize, 20, 100);
        }
        else
        {
            TargetScript.ProfileNameFontSize = EditorGUILayout.IntSlider("Font Size", TargetScript.ProfileNameFontSize, 20, 100);
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Highscore Name Settings", CustomStyles.m_centerBoldLabel);

        GUILayout.Space(10);

        TargetScript.ProfileHighscoreFont = (Font)EditorGUILayout.ObjectField("Highscore Text Font", TargetScript.ProfileHighscoreFont, typeof(Font), true, GUILayout.Height(16));
        TargetScript.ProfileHighscoreFontStyle = (FontStyle)EditorGUILayout.EnumFlagsField("Highscore Text FontStyle", TargetScript.ProfileHighscoreFontStyle);
        TargetScript.ProfileHighscoreBestFit = EditorGUILayout.Toggle("Resize Text For Best Fit", TargetScript.ProfileHighscoreBestFit);

        if (TargetScript.ProfileHighscoreBestFit)
        {
            TargetScript.ProfileHighscoreMinSize = EditorGUILayout.IntSlider("Minimum Font Size", TargetScript.ProfileHighscoreMinSize, 20, 100);
            TargetScript.ProfileHighscoreMaxSize = EditorGUILayout.IntSlider("Maximum Font Size", TargetScript.ProfileHighscoreMaxSize, 20, 100);
        }
        else
        {
            TargetScript.ProfileHighscoreFontSize = EditorGUILayout.IntSlider("Highscore Font Size", TargetScript.ProfileHighscoreFontSize, 20, 100);
        }

        #endregion

        GUILayout.Space(10);

        GUI.color = Color.cyan;

        EditorGUI.BeginDisabledGroup(TargetScript.CategoryList.Count <= 0);
        if (GUILayout.Button("Apply Changes", CustomStyles.m_standardBtn))
            TargetScript.UpdateCategories();
        EditorGUI.EndDisabledGroup();

        GUI.color = Color.white;

        GUILayout.Space(5);
    }

    private void ShowCategories()
    {
#if USE_ADMOB
        if (AdmobManager.ConsentCanvas)
            AdmobManager.ConsentCanvas.SetActive(false);
#endif
        Controller.HomeCanvas.SetActive(false);
        Controller.CategoriesCanvas.SetActive(true);
        Controller.GameCanvas.SetActive(false);
        Controller.PauseCanvas.SetActive(false);
        Controller.GameOverCanvas.SetActive(false);

        Controller.CategoryGroup.SetActive(true);
        Controller.CategoryLoading.gameObject.SetActive(false);
    }

    private void ShowHighscores()
    {
#if USE_ADMOB
        if (AdmobManager.ConsentCanvas)
            AdmobManager.ConsentCanvas.SetActive(false);
#endif
        Controller.ShowHome();
        Controller.ShowHighscores();
    }

    private void ShowHome()
    {
#if USE_ADMOB
        if (AdmobManager.ConsentCanvas)
            AdmobManager.ConsentCanvas.SetActive(false);
#endif
        Controller.ShowHome();
    }

    private void DrawOtherTab()
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField("<b><size=13>Backup & Restore</size></b>", CustomStyles.m_centerBoldLabel, GUILayout.ExpandWidth(true));

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUI.color = Color.cyan;

        if (GUILayout.Button("Backup Categories", CustomStyles.m_standardBtn))
            BackupCategories();

        GUI.color = Color.yellow;

        if (GUILayout.Button("Restore Categories", CustomStyles.m_standardBtn))
            RestoreCategories();

        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        EditorGUILayout.LabelField("<b><size=13>Proceed With Caution</size></b>", CustomStyles.m_centerBoldLabel, GUILayout.ExpandWidth(true));

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUI.color = Color.red;

        if (GUILayout.Button("DELETE ALL CATEGORIES", CustomStyles.m_standardBtn))
        {
            if (TargetScript.CategoryList.Count <= 0)
            {
                EditorUtility.DisplayDialog("Error", "No categories to delete.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Delete ALL Categories", "Are you sure you want to delete ALL categories?", "Yes", "Cancel"))
                TargetScript.CategoryList.Clear();
        }

        GUI.color = Color.white;

        GUILayout.EndHorizontal();
    }

    private void BackupCategories()
    {
        if (TargetScript.CategoryList.Count <= 0)
        {
            EditorUtility.DisplayDialog("Error", "You do not have any categories to backup.", "Ok");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Save Location", "", string.Concat("Backup ", System.DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss tt")), "json");

        if (string.IsNullOrEmpty(path))
            return;

        BackupContainer container = new BackupContainer(TargetScript.CategoryList);

        File.WriteAllText(path, JsonUtility.ToJson(container));

        if (EditorUtility.DisplayDialog("Backup Successful", string.Concat(container.Categories.Count, " categories backed up successfully."), "Open File", "Close"))
            Application.OpenURL(path);
    }

    private void RestoreCategories()
    {
        if (TargetScript.CategoryList.Count > 0)
        {
            if (!EditorUtility.DisplayDialog("Warning", "Restoring categories from a backup file will delete ALL your existing categories.\nDo you want to proceed?", "Yes", "No"))
                return;
        }

        string path = EditorUtility.OpenFilePanel("Backup File", "", "json");

        if (string.IsNullOrEmpty(path))
            return;

        string text = File.ReadAllText(path);

        BackupContainer container = JsonUtility.FromJson<BackupContainer>(text);

        if (container.Categories.Count <= 0)
        {
            EditorUtility.DisplayDialog("Error", "The selected backup file did not contain any categories.", "Ok");
            return;
        }

        TargetScript.CategoryList = container.Categories;

        EditorUtility.DisplayDialog("Restore Successful", string.Concat(container.Categories.Count, " categories restored successfully."), "Ok");
    }
}