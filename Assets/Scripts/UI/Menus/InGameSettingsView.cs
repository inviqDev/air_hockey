using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class InGameSettingsView : MenuViewBase
{
    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.2f;
    [SerializeField] private Ease fadeInEase = Ease.OutSine;
    [SerializeField] private Ease fadeOutEase = Ease.InSine;

    [Header("Settings Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private SureExitView sureExitView;

    [Header("Settings Content")]
    [SerializeField] private GameObject settingsTitleRoot;
    [SerializeField] private GameObject settingsButtonsRoot;

    [Header("Settings Windows")]
    [SerializeField] private GameObject topInfoPanel;
    [SerializeField] private GameObject centerPanel;

    public event Action BackClicked;
    public event Action MainMenuClicked;

    private bool hasCachedManagedViewState;
    private bool wasTopInfoPanelActive;
    private bool wasCenterPanelActive;
    private Tween fadeTween;

    private void OnEnable()
    {
        if (resumeButton)
            resumeButton.onClick.AddListener(HandleBackClicked);

        if (mainMenuButton)
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);

        if (quitButton)
            quitButton.onClick.AddListener(HandleQuitClicked);

        if (sureExitView)
            sureExitView.ExitCancelled += HandleSureExitCancelled;
    }

    private void OnDisable()
    {
        if (resumeButton)
            resumeButton.onClick.RemoveListener(HandleBackClicked);

        if (mainMenuButton)
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);

        if (quitButton)
            quitButton.onClick.RemoveListener(HandleQuitClicked);

        if (sureExitView)
            sureExitView.ExitCancelled -= HandleSureExitCancelled;

        StopFadeTween();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    [ContextMenu("Open Settings")]
    public void Open()
    {
        Show();
    }

    [ContextMenu("Close Settings")]
    public void Close()
    {
        if (sureExitView)
            sureExitView.CancelExit();

        SetSettingsContentActive(true);
        Hide();
    }

    public void ResetState()
    {
        if (sureExitView)
            sureExitView.CancelExit();

        SetSettingsContentActive(true);
        HideImmediately();
    }

    protected override void HandleBeforeShow()
    {
        CacheManagedViewState();
        SetManagedViewUIActive(false);
        SetSettingsContentActive(true);

        if (sureExitView)
            sureExitView.CancelExit();
    }

    protected override void HandleAfterHide()
    {
        RestoreManagedViewState();
    }

    protected override void PlayShowAnimation(Action onComplete)
    {
        StopFadeTween();
        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            0f,
            1f,
            fadeInDuration,
            fadeInEase,
            HandleFadeCompleted(onComplete),
            true);
    }

    protected override void PlayHideAnimation(Action onComplete)
    {
        StopFadeTween();
        fadeTween = MenuAnimationsHelper.PlayCanvasGroupFade(
            ResolvedCanvasGroup,
            1f,
            0f,
            fadeOutDuration,
            fadeOutEase,
            HandleFadeCompleted(onComplete),
            true);
    }

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
        ResetState();
    }

    private void CacheManagedViewState()
    {
        hasCachedManagedViewState = true;
        wasTopInfoPanelActive = topInfoPanel && topInfoPanel.activeSelf;
        wasCenterPanelActive = centerPanel && centerPanel.activeSelf;
    }

    private void RestoreManagedViewState()
    {
        if (!hasCachedManagedViewState) return;

        hasCachedManagedViewState = false;

        if (topInfoPanel)
            topInfoPanel.SetActive(wasTopInfoPanelActive);

        if (centerPanel)
            centerPanel.SetActive(wasCenterPanelActive);
    }

    private void SetManagedViewUIActive(bool isActive)
    {
        if (topInfoPanel)
            topInfoPanel.SetActive(isActive);

        if (centerPanel)
            centerPanel.SetActive(isActive);
    }

    private void SetSettingsContentActive(bool isActive)
    {
        if (settingsTitleRoot)
            settingsTitleRoot.SetActive(isActive);

        if (settingsButtonsRoot)
            settingsButtonsRoot.SetActive(isActive);
    }

    private void HandleBackClicked()
    {
        BackClicked?.Invoke();
        Close();
    }

    private void HandleMainMenuClicked()
    {
        MainMenuClicked?.Invoke();
        Close();
    }

    private void HandleQuitClicked()
    {
        SetSettingsContentActive(false);

        if (sureExitView)
            sureExitView.ShowConfirmation();
    }

    private void HandleSureExitCancelled()
    {
        SetSettingsContentActive(true);
    }

    private void StopFadeTween()
    {
        fadeTween?.Kill();
        fadeTween = null;
    }

    private Action HandleFadeCompleted(Action onComplete)
    {
        return () =>
        {
            fadeTween = null;
            onComplete?.Invoke();
        };
    }

    private void ValidateReferences()
    {
        if (!resumeButton)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a settings back button reference.", this);

        if (!mainMenuButton)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a main menu button reference.", this);

        if (!quitButton)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a quit button reference.", this);

        if (!sureExitView)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a sure exit view reference.", this);

        if (!settingsTitleRoot)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a settings title root reference.", this);

        if (!settingsButtonsRoot)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a settings buttons root reference.", this);

        if (!topInfoPanel)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a top info panel reference.", this);

        if (!centerPanel)
            Debug.LogError($"{nameof(InGameSettingsView)} requires a center panel reference.", this);
    }
}
