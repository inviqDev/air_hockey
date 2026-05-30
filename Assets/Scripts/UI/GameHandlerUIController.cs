using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameHandlerUIController : MonoBehaviour
{
        [Header("Pause")]
        [SerializeField] private Button pausePlayButton;
        [SerializeField] private Image pausePlayIconImage;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite playIcon;
        
        [Header("Top Menu Buttons")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Settings windows")]
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject centerPanel;
        
        [SerializeField] private bool pauseWhenSettingsOpen = true;

        public bool IsPaused { get; private set; }
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

            if (settingsButton)
            {
                settingsButton.onClick.AddListener(OpenSettings);
            }

            if (settingsBackButton)
            {
                settingsBackButton.onClick.AddListener(ResumeGame);
            }

            if (mainMenuButton)
            {
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            }

            if (quitButton)
            {
                quitButton.onClick.AddListener(QuitApplication);
            }
        }

        private void OnDisable()
        {
            if (pausePlayButton)
            {
                pausePlayButton.onClick.RemoveListener(TogglePause);
            }

            if (settingsButton)
            {
                settingsButton.onClick.RemoveListener(OpenSettings);
            }

            if (settingsBackButton)
            {
                settingsBackButton.onClick.RemoveListener(ResumeGame);
            }

            if (mainMenuButton)
            {
                mainMenuButton.onClick.RemoveListener(GoToMainMenu);
            }

            if (quitButton)
            {
                quitButton.onClick.RemoveListener(QuitApplication);
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
            CloseSettings();
            UpdatePauseIcon();
        }

        [ContextMenu("Open Settings")]
        public void OpenSettings()
        {
            if (settingsMenu)
            {
                settingsMenu.SetActive(true);
            }

            if (centerPanel)
            {
                centerPanel.SetActive(false);
            }

            if (pauseWhenSettingsOpen)
            {
                PauseGame();
            }
        }

        [ContextMenu("Close Settings")]
        public void CloseSettings()
        {
            if (settingsMenu)
            {
                settingsMenu.SetActive(false);
            }

            if (centerPanel)
            {
                centerPanel.SetActive(true);
            }
        }

        public void ResetState()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            CloseSettings();
            UpdatePauseIcon();
        }

        private void GoToMainMenu()
        {
            ResetState();
            MainMenuClicked?.Invoke();
        }

        private void QuitApplication()
        {
            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a pause/play button reference.", this);
            }

            if (!pausePlayIconImage)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a pause/play icon image reference.", this);
            }

            if (!pauseIcon)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a pause icon sprite.", this);
            }

            if (!playIcon)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a play icon sprite.", this);
            }

            if (!settingsButton)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a settings button reference.", this);
            }

            if (!settingsMenu)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a settings menu reference.", this);
            }

            if (!centerPanel)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a center panel reference.", this);
            }

            if (!mainMenuButton)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a main menu button reference.", this);
            }

            if (!quitButton)
            {
                Debug.LogError($"{nameof(GameHandlerUIController)} requires a quit button reference.", this);
            }
        }
}
