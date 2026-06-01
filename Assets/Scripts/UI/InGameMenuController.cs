using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class InGameMenuController : MonoBehaviour
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
    [SerializeField] private InGameSettingsMenuController settingsMenuController;
    [SerializeField] private bool pauseWhenSettingsOpen = true;

    public bool IsPaused { get; private set; }
    public event Action RestartClicked;
    public event Action MainMenuClicked;

    private void Awake()
    {
        ValidateReferences();
        ResetState();
    }

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

        if (settingsMenuController)
        {
            settingsMenuController.BackClicked += HandleSettingsBackClicked;
            settingsMenuController.MainMenuClicked += HandleMainMenuClicked;
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

        if (settingsMenuController)
        {
            settingsMenuController.BackClicked -= HandleSettingsBackClicked;
            settingsMenuController.MainMenuClicked -= HandleMainMenuClicked;
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
        IsPaused = true;
        Time.timeScale = 0f;
        UpdatePauseIcon();
    }

    [ContextMenu("Resume Game")]
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UpdatePauseIcon();
    }

    [ContextMenu("Open Settings")]
    public void OpenSettings()
    {
        settingsMenuController?.Open();

        if (pauseWhenSettingsOpen)
        {
            PauseGame();
        }
    }

    public void ResetState()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        settingsMenuController?.ResetState();
        UpdatePauseIcon();
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

        if (!settingsMenuController)
        {
            Debug.LogError($"{nameof(InGameMenuController)} requires a settings menu controller reference.", this);
        }
    }
}
