using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
#endif

public class CategoryCreator : MonoBehaviour
{
#if UNITY_EDITOR
    Regex FilenameRegex = new Regex("^[a-zA-Z0-9]*$");

    public List<CategoryFormat> CategoryList = new List<CategoryFormat>();

    private Controller Controller;

    private Transform CategoriesParent;
    private CategoriesParent CategoriesParentScript;

    private Transform ProfileParent;
    private ProfileParent ProfileParentScript;

    private List<Transform> CurrentCategories = new List<Transform>();
    private List<Transform> CurrentProfileCategories = new List<Transform>();

    private RectTransform CategoryPrefab;
    private RectTransform ProfilePrefab;

    public int CategoryImageSize = 185;
    public int CategoryColumnCount = 3;

    public Font CategoryFont;
    public FontStyle CategoryFontStyle = FontStyle.Normal;
    public bool CategoryBestFit = true;
    public int CategoryFontSize = 40;
    public int CategoryMinSize = 20;
    public int CategoryMaxSize = 50;

    public int ProfileImageSize = 185;
    public int ProfileColumnCount = 3;

    public Font ProfileNameFont;
    public FontStyle ProfileNameFontStyle = FontStyle.Normal;
    public bool ProfileNameBestFit = true;
    public int ProfileNameFontSize = 40;
    public int ProfileNameMinSize = 20;
    public int ProfileNameMaxSize = 50;

    public Font ProfileHighscoreFont;
    public FontStyle ProfileHighscoreFontStyle = FontStyle.Normal;
    public bool ProfileHighscoreBestFit = true;
    public int ProfileHighscoreFontSize = 45;
    public int ProfileHighscoreMinSize = 20;
    public int ProfileHighscoreMaxSize = 50;

    public void Fetch()
    {
        if (!Controller)
            Controller = (Controller)FindObjectOfType(typeof(Controller));

        if (!CategoriesParent)
            CategoriesParent = Controller.CategoryGroup.gameObject.GetComponent<ScrollRect>().content;
        if (!CategoriesParentScript && CategoriesParent)
            CategoriesParentScript = CategoriesParent.gameObject.GetComponent<CategoriesParent>();

        if (!ProfileParent)
            ProfileParent = Controller.HighscorePage.gameObject.GetComponent<ScrollRect>().content;
        if (!ProfileParentScript && ProfileParent)
            ProfileParentScript = ProfileParent.gameObject.GetComponent<ProfileParent>();

        if (!CategoryPrefab)
            CategoryPrefab = (RectTransform)AssetDatabase.LoadAssetAtPath(new string[] { "Assets", "Trivia 365", "Editor", "Prefabs", "CategoryPrefab.prefab" }.Aggregate((x, y) => Path.Combine(x, y)), typeof(RectTransform));
        if (!ProfilePrefab)
            ProfilePrefab = (RectTransform)AssetDatabase.LoadAssetAtPath(new string[] { "Assets", "Trivia 365", "Editor", "Prefabs", "HighscorePrefab.prefab" }.Aggregate((x, y) => Path.Combine(x, y)), typeof(RectTransform));
    }

    public void UpdateCategories()
    {
        Fetch();

        CurrentCategories.Clear();
        CurrentProfileCategories.Clear();
        CategoriesParentScript.Categories.Clear();
        ProfileParentScript.Categories.Clear();

        for (int a = 0; a < CategoriesParent.childCount; a++)
            CurrentCategories.Add(CategoriesParent.GetChild(a));

        foreach (Transform Category in CurrentCategories)
            DestroyImmediate(Category.gameObject);

        for (int a = 0; a < ProfileParent.childCount; a++)
            CurrentProfileCategories.Add(ProfileParent.GetChild(a));

        foreach (Transform Category in CurrentProfileCategories)
            DestroyImmediate(Category.gameObject);

        CategoryController thisCategory;
        ProfileController thisProfileCategory;

        for (int a = 0; a < CategoryList.Count; a++)
        {
            if (!CategoryList[a].Enabled)
                continue;

            if (CategoryList[a].OfflineFile != null)
            {
                if (!TestFilename(CategoryList[a].OfflineFile.name))
                {
                    EditorUtility.DisplayDialog("Error", CategoryList[a].CategoryName + " has an invalid offline filename.\n\nAllowed charcters: Aa-Zz and 0-9", "OK");
                    return;
                }
            }

            #region Category Canvas

            RectTransform CategoryPreset = Instantiate(CategoryPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            CategoryPreset.SetParent(CategoriesParent.transform);
            CategoryPreset.localScale = new Vector3(1, 1, 1);
            CategoryPreset.name = CategoryList[a].CategoryName;

            Image CategoryImage = CategoryPreset.GetChild(0).gameObject.GetComponent<Image>();
            CategoryImage.sprite = CategoryList[a].CategoryImage;

            GridLayoutGroup CategoryGrid = CategoriesParent.GetComponent<GridLayoutGroup>();
            CategoryGrid.cellSize = new Vector2(CategoryImageSize, CategoryImageSize);
            CategoryGrid.constraintCount = CategoryColumnCount;

            Text CategoryName = CategoryPreset.GetChild(1).gameObject.GetComponent<Text>();
            CategoryName.text = CategoryList[a].CategoryName.ToString();

            if (CategoryFont == null)
                CategoryFont = CategoryName.font;

            if (CategoryBestFit)
            {
                CategoryName.resizeTextForBestFit = true;
                CategoryName.resizeTextMinSize = CategoryMinSize;
                CategoryName.resizeTextMaxSize = CategoryMaxSize;
                CategoryName.fontSize = CategoryMaxSize;
            }
            else
            {
                CategoryName.resizeTextForBestFit = false;
                CategoryName.fontSize = CategoryFontSize;
            }

            CategoryName.fontStyle = CategoryFontStyle;
            CategoryName.font = CategoryFont;

            thisCategory = CategoryPreset.GetComponent<CategoryController>();
            thisCategory.Category = CategoryList[a];

            CategoriesParentScript.Categories.Add(thisCategory);

            #endregion

            #region Profile Canvas

            RectTransform ProfilePreset = Instantiate(ProfilePrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            ProfilePreset.SetParent(ProfileParent.transform);
            ProfilePreset.localScale = new Vector3(1, 1, 1);
            ProfilePreset.name = CategoryList[a].CategoryName;

            GridLayoutGroup ProfileGrid = ProfileParent.GetComponent<GridLayoutGroup>();
            ProfileGrid.cellSize = new Vector2(ProfileImageSize, ProfileImageSize);
            ProfileGrid.constraintCount = ProfileColumnCount;

            Text ProfileName = ProfilePreset.GetChild(1).gameObject.GetComponent<Text>();

            if (ProfileNameFont == null)
                ProfileNameFont = ProfileName.font;

            if (ProfileNameBestFit)
            {
                ProfileName.resizeTextForBestFit = true;
                ProfileName.resizeTextMinSize = ProfileNameMinSize;
                ProfileName.resizeTextMaxSize = ProfileNameMaxSize;
                ProfileName.fontSize = ProfileNameMaxSize;
            }
            else
            {
                ProfileName.resizeTextForBestFit = false;
                ProfileName.fontSize = ProfileNameFontSize;
            }

            ProfileName.fontStyle = ProfileNameFontStyle;
            ProfileName.font = ProfileNameFont;

            Text ProfileHighscore = ProfilePreset.GetChild(2).gameObject.GetComponent<Text>();

            if (ProfileHighscoreFont == null)
                ProfileHighscoreFont = ProfileHighscore.font;

            if (ProfileHighscoreBestFit)
            {
                ProfileHighscore.resizeTextForBestFit = true;
                ProfileHighscore.resizeTextMinSize = ProfileHighscoreMinSize;
                ProfileHighscore.resizeTextMaxSize = ProfileHighscoreMaxSize;
                ProfileHighscore.fontSize = ProfileHighscoreMaxSize;
            }
            else
            {
                ProfileHighscore.resizeTextForBestFit = false;
                ProfileHighscore.fontSize = ProfileHighscoreFontSize;
            }

            ProfileHighscore.font = ProfileHighscoreFont;
            ProfileHighscore.fontStyle = ProfileHighscoreFontStyle;

            thisProfileCategory = ProfilePreset.GetComponent<ProfileController>();
            thisProfileCategory.CategoryImage.sprite = CategoryList[a].CategoryImage;
            thisProfileCategory.CategoryName.text = CategoryList[a].CategoryName;
            thisProfileCategory.HighscorePref = CategoryList[a].HighscorePref;
            thisProfileCategory.Language = CategoryList[a].Language;

            ProfileParentScript.Categories.Add(thisProfileCategory);

            #endregion
        }

        ProfileParentScript.AlignGameCategories();
        CategoriesParentScript.AlignGameCategories();
        GUIUtility.keyboardControl = 0;
    }

    public void OnlineTest(string CategoryName, string Link)
    {
        StopAllCoroutines();
        StartCoroutine(DownloadFile(CategoryName, Link));
    }

    public IEnumerator DownloadFile(string CategoryName, string Link)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(Link))
        {
            uwr.timeout = 30;

            yield return uwr.SendWebRequest();

            if (uwr.isHttpError || uwr.isNetworkError)
            {
                EditorUtility.DisplayDialog("Error", "Download failed.", "OK");
                Debug.LogError(uwr.error);
            }
            else
            {
                OfflineTest(CategoryName, uwr.downloadHandler.text);
            }
        }
    }

    public void OfflineTest(string CategoryName, string Content)
    {
        QuestionsContainer container = JsonUtility.FromJson<QuestionsContainer>(Content);

        EditorUtility.DisplayDialog(CategoryName, "We found " + container.Questions.Count + " questions.", "OK");
    }

    public bool TestFilename(string Name)
    {
        return FilenameRegex.IsMatch(Name);
    }
#endif
}

//Debug.Log(Path.GetExtension(AssetDatabase.GetAssetPath(format.OfflineFile)).ToLower());