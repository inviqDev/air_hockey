using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class InGameMenu : MenuViewBase
{
    [Header("Top Menu Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button pausePlayButton;
    [SerializeField] private Button restartButton;

    [Header("Pause Button Icons")]
    [SerializeField] private Image pausePlayIconImage;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite playIcon;

    [Header("Settings Flow")]
    [SerializeField] private InGameSettingsView settingsView;

    public bool IsPaused => currentOverlay == GameOverlay.Pause;
    public bool IsSettingsOpen => currentOverlay == GameOverlay.Settings;
    public event Action RestartClicked;
    public event Action MainMenuClicked;
    public event Action PauseToggleRequested;
    public event Action SettingsOpenRequested;
    public event Action SettingsCloseRequested;

    private GameOverlay currentOverlay;

    private void OnEnable()
    {
        if (pausePlayButton)
        {
            pausePlayButton.onClick.AddListener(TogglePause);
        }

        if (restartButton)
        {
            restartButton.onClick.AddListener(RestartMatch);
        }

        if (settingsButton)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (settingsView)
        {
            settingsView.BackClicked += HandleSettingsBackClicked;
            settingsView.MainMenuClicked += HandleMainMenuClicked;
        }
    }

    private void OnDisable()
    {
        if (pausePlayButton)
        {
            pausePlayButton.onClick.RemoveListener(TogglePause);
        }

        if (restartButton)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
        }

        if (settingsButton)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
        }

        if (settingsView)
        {
            settingsView.BackClicked -= HandleSettingsBackClicked;
            settingsView.MainMenuClicked -= HandleMainMenuClicked;
        }

        ApplyOverlayVisualState(GameOverlay.None);
    }

    private void OnValidate()
    {
        ValidateReferences();
        UpdatePauseIcon();
    }

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
        ResetState();
    }

    [ContextMenu("Toggle Pause")]
    public void TogglePause()
    {
        PauseToggleRequested?.Invoke();
    }

    [ContextMenu("Pause Game")]
    public void PauseGame()
    {
        PauseToggleRequested?.Invoke();
    }

    [ContextMenu("Resume Game")]
    public void ResumeGame()
    {
        PauseToggleRequested?.Invoke();
    }

    [ContextMenu("Open Settings")]
    public void OpenSettings()
    {
        SettingsOpenRequested?.Invoke();
    }

    public void ResetState()
    {
        ApplyOverlayState(GameOverlay.None);
    }

    public void ApplyOverlayState(GameOverlay overlay)
    {
        currentOverlay = overlay;
        ApplyOverlayVisualState(currentOverlay);
    }

    private void RestartMatch()
    {
        RestartClicked?.Invoke();
    }

    private void HandleSettingsBackClicked()
    {
        SettingsCloseRequested?.Invoke();
    }

    private void HandleMainMenuClicked()
    {
        MainMenuClicked?.Invoke();
    }

    private void UpdatePauseIcon()
    {
        if (!pausePlayIconImage) return;

        pausePlayIconImage.sprite = IsPaused ? playIcon : pauseIcon;
    }

    private void ApplyOverlayVisualState(GameOverlay overlay)
    {
        var isBlockingGameplay = overlay != GameOverlay.None;
        Time.timeScale = isBlockingGameplay ? 0f : 1f;

        if (overlay == GameOverlay.Settings)
        {
            if (settingsView && !settingsView.IsVisible)
                settingsView.Open();
        }
        else if (settingsView && settingsView.IsVisible)
        {
            settingsView.Close();
        }

        UpdatePauseIcon();
    }

    private void ValidateReferences()
    {
        if (!pausePlayButton)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a pause/play button reference.", this);
        }

        if (!pausePlayIconImage)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a pause/play icon image reference.", this);
        }

        if (!pauseIcon)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a pause icon sprite.", this);
        }

        if (!playIcon)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a play icon sprite.", this);
        }

        if (!restartButton)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a restart button reference.", this);
        }

        if (!settingsButton)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a settings button reference.", this);
        }

        if (!settingsView)
        {
            Debug.LogError($"{nameof(InGameMenu)} requires a settings menu controller reference.", this);
        }
    }
}
