using System.Collections.Generic;
using UnityEngine;

public class ProfileParent : MonoBehaviour
{
    [HideInInspector]
    public List<ProfileController> Categories = new List<ProfileController>();

    private LanguageList CurrentLanguage;

    private void Start()
    {
        SetupCategories(true);
        AlignGameCategories();
    }

    private void OnEnable()
    {
        LocalizationManager.onLanguageChange += SetupCategories;
    }

    private void SetupCategories(bool Force = false)
    {
        int Activated = 0;

        if (CurrentLanguage == LocalizationManager.instance.CurrentLanguage && !Force)
            return;

        CurrentLanguage = LocalizationManager.instance.CurrentLanguage;

        for (int x = 0; x < Categories.Count; x++)
        {
            if (Categories[x].Language == CurrentLanguage)
            {
                Categories[x].gameObject.SetActive(true);
                Activated++;
            }
            else
                Categories[x].gameObject.SetActive(false);
        }

        if (Activated == 0)
        {
            for (int x = 0; x < Categories.Count; x++)
                Categories[x].gameObject.SetActive(true);
        }
    }

    internal void AlignGameCategories()
    {
        RectTransform Rect = gameObject.GetComponent<RectTransform>();

        if (!Rect)
            return;

        Rect.anchorMin = new Vector2(0, 1);
        Rect.anchorMax = new Vector2(1, 1);
        Rect.pivot = new Vector2(0.5f, 1);

        Rect.anchoredPosition = new Vector2(0, 0);
        Rect.sizeDelta = new Vector3(0, 0, 0);
    }

    private void OnDestroy()
    {
        LocalizationManager.onLanguageChange -= SetupCategories;
    }
}
