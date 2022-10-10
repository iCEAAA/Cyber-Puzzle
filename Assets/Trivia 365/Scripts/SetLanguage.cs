using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("QuizApp/Set Language")]
[RequireComponent(typeof(Button))]
public class SetLanguage : MonoBehaviour
{
    public LanguageList Language;

    private Button ButtonComponent;

    private void Awake()
    {
        ButtonComponent = gameObject.GetComponent<Button>();
        
        ButtonComponent.onClick.AddListener(() =>
        {
            ChangeLanguage();
        });
    }

    private void ChangeLanguage()
    {
        LocalizationManager.instance.ChangeLanguage(Language);
    }

    void OnDestroy()
    {
        ButtonComponent.onClick.RemoveAllListeners();
    }
}
