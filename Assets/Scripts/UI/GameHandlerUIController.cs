using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class GameHandlerUIController : MonoBehaviour
    {
        [Header("Pause")]
        [SerializeField] private Button pausePlayButton;
        [SerializeField] private Image pausePlayIconImage;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite playIcon;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject centerPanel;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private bool pauseWhenSettingsOpen = true;

        public bool IsPaused { get; private set; }

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
        }
    }
}
