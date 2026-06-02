using DG.Tweening;
using UnityEngine;

public abstract class MenuViewBase : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    
    [Header("Visibility Animation")]
    [SerializeField] private bool animateVisibility = true;
    [SerializeField, Min(0.01f)] private float showDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float hideDuration = 0.25f;
    [SerializeField] private Ease showEase = Ease.OutQuad;
    [SerializeField] private Ease hideEase = Ease.InQuad;

    public bool IsVisible => menuRoot && menuRoot.activeSelf;

    private CanvasGroup canvasGroup;
    private Tween visibilityTween;

    protected virtual void Awake()
    {
        ResolveRoot();
        ResolveCanvasGroup();
    }

    public void Show()
    {
        ResolveRoot();
        ResolveCanvasGroup();
        visibilityTween?.Kill();

        HandleBeforeShow();

        if (menuRoot)
        {
            menuRoot.SetActive(true);
        }

        if (!animateVisibility || !canvasGroup)
        {
            SetCanvasState(1f, true);
            SetRootActive(true);
            HandleAfterShow();
            return;
        }

        SetRootActive(true);
        SetCanvasState(0f, true);
        visibilityTween = canvasGroup
            .DOFade(1f, showDuration)
            .SetEase(showEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                visibilityTween = null;
                HandleAfterShow();
            });
    }

    public void Hide()
    {
        ResolveRoot();
        ResolveCanvasGroup();
        visibilityTween?.Kill();
        HandleBeforeHide();

        if (!animateVisibility || !canvasGroup)
        {
            SetCanvasState(0f, false);
            SetRootActive(false);
            HandleAfterHide();
            return;
        }

        SetCanvasState(1f, false);
        visibilityTween = canvasGroup
            .DOFade(0f, hideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                visibilityTween = null;
                SetCanvasState(0f, false);
                SetRootActive(false);
                HandleAfterHide();
            });
    }

    protected virtual void HandleBeforeShow()
    {
    }

    protected virtual void HandleAfterShow()
    {
    }

    protected virtual void HandleBeforeHide()
    {
    }

    protected virtual void HandleAfterHide()
    {
    }

    private void ResolveRoot()
    {
        if (menuRoot) return;
        
        Debug.LogError($"{gameObject.name} object root is  not set the inspector");
        menuRoot = gameObject;
    }

    private void ResolveCanvasGroup()
    {
        if (!menuRoot || canvasGroup) return;

        if (menuRoot.TryGetComponent<CanvasGroup>(out var resolvedCanvasGroup))
        {
            canvasGroup = resolvedCanvasGroup;
            return;
        }

        canvasGroup = menuRoot.AddComponent<CanvasGroup>();
    }

    private void SetCanvasState(float alpha, bool active)
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
    }

    private void SetRootActive(bool active)
    {
        if (!menuRoot) return;

        menuRoot.SetActive(active);
    }
}