using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class InGameSettingsMenuController : MonoBehaviour
{
    [Header("Settings Menu Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Windows")]
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject topInfoPanel;
    [SerializeField] private GameObject centerPanel;

    public event Action BackClicked;
    public event Action MainMenuClicked;

    private void Awake()
    {
        ValidateReferences();
        ResetState();
    }

    private void OnEnable()
    {
        if (backButton)
        {
            backButton.onClick.AddListener(HandleBackClicked);
        }

        if (mainMenuButton)
        {
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        if (quitButton)
        {
            quitButton.onClick.AddListener(QuitApplication);
        }
    }

    private void OnDisable()
    {
        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        if (mainMenuButton)
        {
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }

        if (quitButton)
        {
            quitButton.onClick.RemoveListener(QuitApplication);
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    [ContextMenu("Open Settings")]
    public void Open()
    {
        if (settingsMenu)
        {
            settingsMenu.SetActive(true);
        }

        ManageInGameViewUI(false);
    }

    private void ManageInGameViewUI(bool isActive)
    {
        if (topInfoPanel)
        {
            topInfoPanel.SetActive(isActive);
        }
        
        if (centerPanel)
        {
            centerPanel.SetActive(isActive);
        }
    }

    [ContextMenu("Close Settings")]
    public void Close()
    {
        if (settingsMenu)
        {
            settingsMenu.SetActive(false);
        }

        ManageInGameViewUI(true);
    }

    public void ResetState()
    {
        Close();
    }

    private void HandleBackClicked()
    {
        Close();
        BackClicked?.Invoke();
    }

    private void HandleMainMenuClicked()
    {
        ResetState();
        MainMenuClicked?.Invoke();
    }

    private static void QuitApplication()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ValidateReferences()
    {
        if (!backButton)
        {
            Debug.LogError($"{nameof(InGameSettingsMenuController)} requires a settings back button reference.", this);
        }

        if (!mainMenuButton)
        {
            Debug.LogError($"{nameof(InGameSettingsMenuController)} requires a main menu button reference.", this);
        }

        if (!quitButton)
        {
            Debug.LogError($"{nameof(InGameSettingsMenuController)} requires a quit button reference.", this);
        }

        if (!settingsMenu)
        {
            Debug.LogError($"{nameof(InGameSettingsMenuController)} requires a settings menu reference.", this);
        }

        if (!centerPanel)
        {
            Debug.LogError($"{nameof(InGameSettingsMenuController)} requires a center panel reference.", this);
        }
    }
}
