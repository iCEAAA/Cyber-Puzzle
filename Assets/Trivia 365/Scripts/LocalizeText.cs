using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("QuizApp/Localize Text")]
[RequireComponent(typeof(Text))]
public class LocalizeText : MonoBehaviour
{
    private LanguageList CurrentLanguage = 0;
    private Text textComponent;

    public string Key;
    public bool LocalizeOnStart = true;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<Text>();
    }

    private void Start()
    {
        if (LocalizeOnStart)
            Localize(true);
    }

    private void OnEnable()
    {
        LocalizationManager.onLanguageChange += Localize;
    }

    internal void UpdateKey(string NewKey)
    {
        if (Key.Equals(NewKey))
            return;

        Key = NewKey;
        Localize(true);
    }

    private void Localize(bool Force = false)
    {
        if (CurrentLanguage == LocalizationManager.instance.CurrentLanguage && !Force)
            return;

        textComponent.text = LocalizationManager.instance.GetLocalizedString(Key);

        CurrentLanguage = LocalizationManager.instance.CurrentLanguage;
    }

    private void OnDestroy()
    {
        LocalizationManager.onLanguageChange -= Localize;
    }
}
