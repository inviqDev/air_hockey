using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class InGameMenuController : MenuViewBase
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
    [SerializeField] private bool pauseWhenSettingsOpen = true;

    public bool IsPaused { get; private set; }
    public event Action RestartClicked;
    public event Action MainMenuClicked;
    public event Action<bool> PauseStateChanged;

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

        if (IsPaused)
        {
            ResumeGame();
        }
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
        if (IsPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    [ContextMenu("Pause Game")]
    public void PauseGame()
    {
        SetPaused(true);
    }

    [ContextMenu("Resume Game")]
    public void ResumeGame()
    {
        SetPaused(false);
    }

    [ContextMenu("Open Settings")]
    public void OpenSettings()
    {
        settingsView?.Open();

        if (pauseWhenSettingsOpen)
        {
            PauseGame();
        }
    }

    public void ResetState()
    {
        var wasPaused = IsPaused;
        IsPaused = false;
        ApplyPauseState();
        settingsView?.ResetState();

        if (wasPaused)
            PauseStateChanged?.Invoke(IsPaused);
    }

    private void RestartMatch()
    {
        ResetState();
        RestartClicked?.Invoke();
    }

    private void HandleSettingsBackClicked()
    {
        if (pauseWhenSettingsOpen)
        {
            ResumeGame();
        }
    }

    private void HandleMainMenuClicked()
    {
        ResetState();
        MainMenuClicked?.Invoke();
    }

    private void UpdatePauseIcon()
    {
        if (!pausePlayIconImage) return;

        pausePlayIconImage.sprite = IsPaused ? playIcon : pauseIcon;
    }

    private void SetPaused(bool isPaused)
    {
        if (IsPaused == isPaused) return;

        IsPaused = isPaused;
        ApplyPauseState();
        PauseStateChanged?.Invoke(IsPaused);
    }

    private void ApplyPauseState()
    {
        Time.timeScale = IsPaused ? 0f : 1f;
        UpdatePauseIcon();
    }

    private void ValidateReferences()
    {
        if (!pausePlayButton)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a pause/play button reference.", this);
        }

        if (!pausePlayIconImage)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a pause/play icon image reference.", this);
        }

        if (!pauseIcon)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a pause icon sprite.", this);
        }

        if (!playIcon)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a play icon sprite.", this);
        }

        if (!restartButton)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a restart button reference.", this);
        }

        if (!settingsButton)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a settings button reference.", this);
        }

        if (!settingsView)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a settings menu controller reference.", this);
        }
    }
}
