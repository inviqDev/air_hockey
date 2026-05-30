using System;
using DG.Tweening;
using UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuRoot;
        [SerializeField] private Button aiOpponentButton;
        [SerializeField] private Button secondPlayerButton;

        [Header("Animation")]
        [SerializeField, Range(0.05f, 3f)] private float fadeInDuration = 1f;
        [SerializeField, Range(0.05f, 3f)] private float fadeOutDuration = 1f;

        public event Action<MainMenuSelection> SelectionMade;

        private CanvasGroup mainMenuCanvasGroup;
        private Tween mainMenuTween;

        private void Awake()
        {
            ValidateReferences();
            ResolveMainMenuRoot();
        }

        private void OnEnable()
        {
            AddButtonListeners();
        }

        private void OnDisable()
        {
            mainMenuTween?.Kill();
            mainMenuTween = null;
            RemoveButtonListeners();
        }

        private void OnValidate()
        {
            ValidateReferences();
        }

        public void Show()
        {
            if (!mainMenuRoot) return;

            ResolveMainMenuRoot();
            mainMenuTween?.Kill();
            mainMenuRoot.SetActive(true);

            if (!mainMenuCanvasGroup) return;

            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;

            mainMenuTween = mainMenuCanvasGroup
                .DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void Hide()
        {
            if (!mainMenuRoot) return;

            ResolveMainMenuRoot();
            mainMenuTween?.Kill();

            if (!mainMenuCanvasGroup)
            {
                mainMenuRoot.SetActive(false);
                return;
            }

            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;

            mainMenuTween = mainMenuCanvasGroup
                .DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() => mainMenuRoot.SetActive(false));
        }

        private void SelectAiOpponent()
        {
            Select(MainMenuSelection.AiOpponent);
        }

        private void SelectSecondPlayer()
        {
            Select(MainMenuSelection.SecondPlayer);
        }

        private void Select(MainMenuSelection selection)
        {
            Hide();
            SelectionMade?.Invoke(selection);
        }

        private void AddButtonListeners()
        {
            if (aiOpponentButton)
            {
                aiOpponentButton.interactable = true;
            
                aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
                aiOpponentButton.onClick.AddListener(SelectAiOpponent);
            }

            if (secondPlayerButton)
            {
                secondPlayerButton.interactable = true;
            
                secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
                secondPlayerButton.onClick.AddListener(SelectSecondPlayer);
            }
        }

        private void RemoveButtonListeners()
        {
            if (aiOpponentButton)
            {
                aiOpponentButton.interactable = false;
                aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
            }

            if (secondPlayerButton)
            {
                secondPlayerButton.interactable = false;
                secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
            }
        }

        private void ResolveMainMenuRoot()
        {
            if (!mainMenuRoot)
            {
                mainMenuRoot = gameObject;
            }

            if (!mainMenuCanvasGroup)
            {
                mainMenuCanvasGroup = mainMenuRoot.GetComponent<CanvasGroup>();
            }

            if (!mainMenuCanvasGroup)
            {
                mainMenuCanvasGroup = mainMenuRoot.AddComponent<CanvasGroup>();
            }
        }

        private void ValidateReferences()
        {
            if (!aiOpponentButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires an AI opponent button reference.", this);
            }

            if (!secondPlayerButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires a second player button reference.", this);
            }
        }
    }
}
