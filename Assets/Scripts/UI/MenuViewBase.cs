using UnityEngine;

public abstract class MenuViewBase : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;

    public bool IsVisible => menuRoot && menuRoot.activeSelf;

    protected virtual void Awake()
    {
        ResolveRoot();
    }

    public void Show()
    {
        ResolveRoot();

        if (menuRoot)
        {
            menuRoot.SetActive(true);
        }

        OnShown();
    }

    public void Hide()
    {
        OnHidden();

        if (menuRoot)
        {
            menuRoot.SetActive(false);
        }
    }

    protected virtual void OnShown()
    {
    }

    protected virtual void OnHidden()
    {
    }

    private void ResolveRoot()
    {
        if (menuRoot) return;
        
        Debug.LogError($"{gameObject.name} object root is  not set the inspector");
        menuRoot = gameObject;
    }
}
