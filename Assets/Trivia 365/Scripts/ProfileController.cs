using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    public Image CategoryImage;
    public Text CategoryName;
    public Text HighscoreText;

    [HideInInspector]
    public string HighscorePref;
    [HideInInspector]
    public LanguageList Language;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(HighscorePref))
            return;

        HighscoreText.text = PlayerPrefs.GetInt(HighscorePref, 0).ToString();
    }
}
