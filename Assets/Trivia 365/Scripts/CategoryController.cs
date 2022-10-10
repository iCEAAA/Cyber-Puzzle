using UnityEngine;
using UnityEngine.UI;

public class CategoryController : MonoBehaviour
{
    [HideInInspector]
    public CategoryFormat Category;
    private Button thisButton;

    private void Awake()
    {
        thisButton = gameObject.GetComponent<Button>();
        thisButton.onClick.AddListener(() => { LoadCategory(); });
    }

    private void LoadCategory()
    {
        Controller.instance.LoadCategory(Category);
    }

    private void OnDestroy()
    {
        thisButton.onClick.RemoveAllListeners();
    }
}