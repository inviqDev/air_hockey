using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuViewBase : MonoBehaviour
{
    [SerializeField] private bool visibleByDefault;

    public bool IsVisible => gameObject.activeSelf;
    public bool IsTransitioning { get; private set; }

    private CanvasGroup canvasGroup;
    private bool isInitialized;

    protected CanvasGroup ResolvedCanvasGroup => canvasGroup;

    public void Initialize()
    {
        if (isInitialized) return;

        ResolveCanvasGroup();
        IsTransitioning = false;
        
        gameObject.SetActive(false);
        
        ResetToDefault();
        HandleAfterInitialize();

        isInitialized = true;
    }

    public void Show()
    {
        PrepareViewForAppearance();
        HandleBeforeShow();

        SetCanvasVisibleState(1f, true);
        IsTransitioning = true;

        PlayShowAnimation(CompleteShowTransition);
    }

    public void ShowImmediately()
    {
        PrepareViewForAppearance();
        HandleBeforeShow();

        IsTransitioning = false;

        ApplyVisibleState();
        HandleAfterShow();
    }

    public void Hide()
    {
        ResolveCanvasGroup();
        HandleBeforeHide();

        SetCanvasVisibleState(1f, false);
        IsTransitioning = true;

        PlayHideAnimation(CompleteHideTransition);
    }

    public void HideImmediately()
    {
        ResolveCanvasGroup();
        HandleBeforeHide();

        IsTransitioning = false;

        ApplyHiddenState();
        HandleAfterHide();
    }

    public void ResetToDefault()
    {
        ResolveCanvasGroup();
        HandleBeforeResetToDefault();

        if (visibleByDefault)
            ShowImmediately();
        else
            HideImmediately();

        HandleAfterResetToDefault();
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

    protected virtual void HandleBeforeResetToDefault()
    {
    }

    protected virtual void HandleAfterResetToDefault()
    {
    }

    protected virtual void HandleAfterInitialize()
    {
    }

    protected virtual void PlayShowAnimation(System.Action onComplete)
    {
        onComplete?.Invoke();
    }

    protected virtual void PlayHideAnimation(System.Action onComplete)
    {
        onComplete?.Invoke();
    }

    protected void SetCanvasVisibleState(float alpha, bool isInteractive)
    {
        if (!canvasGroup) return;

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = isInteractive;
        canvasGroup.blocksRaycasts = isInteractive;
    }

    private void PrepareViewForAppearance()
    {
        ResolveCanvasGroup();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    private void ResolveCanvasGroup()
    {
        if (canvasGroup) return;

        canvasGroup = GetComponent<CanvasGroup>();

        if (!canvasGroup)
            Debug.LogError($"{nameof(MenuViewBase)} on {name} requires a " +
                           $"{nameof(CanvasGroup)} on the same GameObject.", this);
    }

    private void ApplyVisibleState()
    {
        SetCanvasVisibleState(1f, true);
    }

    private void ApplyHiddenState()
    {
        SetCanvasVisibleState(0f, false);
        SetViewRootActive(false);
    }

    private void CompleteShowTransition()
    {
        IsTransitioning = false;
        ApplyVisibleState();
        HandleAfterShow();
    }

    private void CompleteHideTransition()
    {
        IsTransitioning = false;
        ApplyHiddenState();
        HandleAfterHide();
    }

    private void SetViewRootActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
