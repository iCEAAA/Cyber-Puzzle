using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Networking;
#endif

public class LocalizationManager : MonoBehaviour
{
    internal static LocalizationManager instance;

    public List<string> Languages = new List<string>();
    public LanguageList CurrentLanguage = 0;

    public delegate void LanguageChanged(bool Force = false);
    public static event LanguageChanged onLanguageChange;

    private const string LanguagePref = "ActiveLanguage";

    private Dictionary<string, string> LocalizationData = new Dictionary<string, string>();

#if UNITY_EDITOR
    public List<LocaleFormat> Localization = new List<LocaleFormat>();
    public List<string> LocalizationKeys = new List<string>();
    public string GoogleSheetID;
    public string GoogleSheetGid;
    public string SpreadsheetData;
    private string path;

    public bool DisableImportButtons = false;
    public bool MinifyJson = true;
#endif

    private void Awake()
    {
        if (instance == null)
            instance = this;

        int SavedPref = PlayerPrefs.GetInt(LanguagePref, 0);

        if (SavedPref >= Languages.Count)
        {
            SavedPref = 0;
            PlayerPrefs.SetInt(LanguagePref, SavedPref);
        }

        CurrentLanguage = (LanguageList)SavedPref;

        if (Languages.Count > 1)
            LoadLanguageFile(true);
    }

    private void LoadLanguageFile(bool Force = false)
    {
        TextAsset LocalizationFile = Resources.Load<TextAsset>("i18n/" + Languages[(int)CurrentLanguage]);

        if (LocalizationFile == null)
        {
            Debug.LogError(string.Concat("Unable to load ", Languages[(int)CurrentLanguage], " localization file"));
            return;
        }

        LocalizationContainer container = JsonUtility.FromJson<LocalizationContainer>(LocalizationFile.text);

        LocalizationData.Clear();

        for (int i = 0; i < container.Locale.Count; i++)
            LocalizationData.Add(container.Locale[i].Key, container.Locale[i].Value);

        if (onLanguageChange != null)
            onLanguageChange(Force);

        Resources.UnloadUnusedAssets();

        //if(!Force)
        //    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    internal void ChangeLanguage(LanguageList language = 0)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;

        PlayerPrefs.SetInt(LanguagePref, (int)language);

        LoadLanguageFile();
    }

    internal string GetLocalizedString(string key)
    {
        if (LocalizationData.ContainsKey(key))
            return LocalizationData[key];
        else
            return "Missing!";
    }

#if UNITY_EDITOR
    public void ExportCSV()
    {
        string path = EditorUtility.SaveFilePanel("Save Location", "", "Localization Template", "csv");

        if (string.IsNullOrEmpty(path))
            return;

        string fileBody = string.Empty;

        fileBody = "Key";

        foreach (string str in Languages)
            fileBody += string.Concat(",", str);

        File.WriteAllText(path, fileBody);

        if (EditorUtility.DisplayDialog("Save Successful", "Localization file saved successfully.", "Open File", "Close"))
            Application.OpenURL(path);
    }

    public void ImportCSV(string path = null, bool local = true)
    {
        if (local)
            path = EditorUtility.OpenFilePanel("Open File", "", "csv");

        if (string.IsNullOrEmpty(path))
            return;

        Localization.Clear();

        string[] content;

        if (local)
        {
            try
            {
                content = File.ReadAllLines(path);
            }
            catch (IOException)
            {
                EditorApplication.Beep();

                EditorUtility.DisplayDialog("Error", "The selected file might be in use or you do not have sufficient permissions to access it. Close any application that might be using the file, and then try again. You might need to restart your computer.", "OK");
                return;
            }
        }
        else
            content = path.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);

        string[] sheetLanguages = content[0].Split(new[] { "," }, System.StringSplitOptions.None);

        List<string> sheetLangs = sheetLanguages.Skip(1).ToList();

        if (Languages.Count > sheetLangs.Count)
        {
            EditorApplication.Beep();

            EditorUtility.DisplayDialog("Import Error", string.Concat("The imported localization file is missing ", Languages.Count - sheetLangs.Count, " language(s)."), "OK");
            return;
        }
        else if (Languages.Count < sheetLangs.Count)
        {
            EditorApplication.Beep();

            if (!EditorUtility.DisplayDialog("Warning", string.Concat("The imported localization file has ", sheetLangs.Count - Languages.Count, " extra language(s)."), "Continue", "Cancel"))
            {
                EditorUtility.DisplayDialog("Import Cancelled", "The import process has been cancelled.", "OK");
                return;
            }
        }

        Localization = content.Skip(1).Select(v => LocaleFormat.FromCsv(v)).ToList();

        string[] SaveFolder = Directory.GetDirectories(Application.dataPath, "i18n", SearchOption.AllDirectories);

        if (SaveFolder.Length <= 0)
        {
            string createPath = new string[] { Application.dataPath, "Trivia 365", "Resources", "i18n" }.Aggregate((x, y) => Path.Combine(x, y));
            Directory.CreateDirectory(createPath);

            SaveFolder = new string[1] { createPath };
        }

        LocalizationKeys.Clear();

        for (int lang = 0; lang < Languages.Count; lang++)
        {
            LocalizationContainer container = new LocalizationContainer();
            List<LocalizationFormat> format = new List<LocalizationFormat>();

            for (int x = 0; x < Localization.Count; x++)
                format.Add(new LocalizationFormat(Localization[x].Key, Localization[x].Languages[lang]));

            container = new LocalizationContainer(format);
            File.WriteAllText(Path.Combine(SaveFolder[0], string.Concat(Languages[lang], ".json")), JsonUtility.ToJson(container, !MinifyJson));
        }

        LocalizationKeys.Add(string.Empty);

        for (int x = 0; x < Localization.Count; x++)
            LocalizationKeys.Add(Localization[x].Key);

        LocalizationKeys.Sort();

        AssetDatabase.Refresh();

        EditorUtility.SetDirty(this);

        EditorUtility.DisplayDialog("Success", "Localization files created successfully.", "OK");
    }

    public void StartDownload()
    {
        if (string.IsNullOrEmpty(GoogleSheetID))
        {
            EditorUtility.DisplayDialog("Error", "The Google Sheet ID is not set.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(GoogleSheetGid))
        {
            EditorUtility.DisplayDialog("Error", "The Google Sheet GID number is not set.", "OK");
            return;
        }

        StartCoroutine(DownloadSpreadsheet());
    }

    private IEnumerator DownloadSpreadsheet()
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/" + GoogleSheetID + "/export?format=csv&id=" + GoogleSheetID + "&gid=" + GoogleSheetGid))
        {

            uwr.timeout = 20;

            uwr.SetRequestHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
            uwr.SetRequestHeader("cache-control", "no-cache");
            uwr.SetRequestHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            DisableImportButtons = true;

            yield return uwr.SendWebRequest();

            DisableImportButtons = false;

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                EditorUtility.DisplayDialog("Download Error", "An error occurred during download.\n\nCheck the console for more information.", "OK");
                Debug.LogError("SPREADSHEET DOWNLOAD ERROR.\n\nResponse Code - " + uwr.responseCode + "\n\nError Message - " + uwr.error + "\n");
            }
            else
                ImportCSV(uwr.downloadHandler.text, false);
        }
    }
#endif
}