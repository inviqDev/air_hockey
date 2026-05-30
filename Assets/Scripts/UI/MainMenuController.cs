using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
        [SerializeField] private GameObject mainMenuRoot;
        [SerializeField] private GameObject opponentSelectionRoot;
        [SerializeField] private GameObject sideSelectionRoot;
        [SerializeField] private Button aiOpponentButton;
        [SerializeField] private Button secondPlayerButton;
        [SerializeField] private Button leftSideButton;
        [SerializeField] private Button rightSideButton;

        [Header("Animation")]
        [SerializeField, Range(0.05f, 3f)] private float fadeInDuration = 1f;
        [SerializeField, Range(0.05f, 3f)] private float fadeOutDuration = 1f;

        public event Action<MatchConfiguration> MatchConfigurationSelected;

        private CanvasGroup mainMenuCanvasGroup;
        private Tween mainMenuTween;
        private PlayerTwoControlType selectedPlayerTwoControlType;

        private void Awake()
        {
            ValidateReferences();
            ResolveMainMenuRoot();
            SetOpponentSelectionVisible(true);
            SetSideSelectionVisible(false);
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
            SetOpponentSelectionVisible(true);
            SetSideSelectionVisible(false);

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
            SelectMode(PlayerTwoControlType.Ai);
        }

        private void SelectSecondPlayer()
        {
            SelectMode(PlayerTwoControlType.Human);
        }

        private void SelectMode(PlayerTwoControlType playerTwoControlType)
        {
            selectedPlayerTwoControlType = playerTwoControlType;
            SetOpponentSelectionVisible(false);
            SetSideSelectionVisible(true);
        }

        private void SelectLeftSide()
        {
            SelectSide(PlayerSide.Left);
        }

        private void SelectRightSide()
        {
            SelectSide(PlayerSide.Right);
        }

        private void SelectSide(PlayerSide playerOneSide)
        {
            SetSideSelectionVisible(false);
            SetOpponentSelectionVisible(false);
            Hide();
            MatchConfigurationSelected?.Invoke(new MatchConfiguration(selectedPlayerTwoControlType, playerOneSide));
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

            if (leftSideButton)
            {
                leftSideButton.onClick.RemoveListener(SelectLeftSide);
                leftSideButton.onClick.AddListener(SelectLeftSide);
            }

            if (rightSideButton)
            {
                rightSideButton.onClick.RemoveListener(SelectRightSide);
                rightSideButton.onClick.AddListener(SelectRightSide);
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

            if (leftSideButton)
            {
                leftSideButton.onClick.RemoveListener(SelectLeftSide);
            }

            if (rightSideButton)
            {
                rightSideButton.onClick.RemoveListener(SelectRightSide);
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
            if (!opponentSelectionRoot)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires an opponent selection root reference.", this);
            }

            if (!sideSelectionRoot)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires a side selection root reference.", this);
            }

            if (!aiOpponentButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires an AI opponent button reference.", this);
            }

            if (!secondPlayerButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires a second player button reference.", this);
            }

            if (!leftSideButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires a left side button reference.", this);
            }

            if (!rightSideButton)
            {
                Debug.LogError($"{nameof(MainMenuController)} requires a right side button reference.", this);
            }
        }

        private void SetOpponentSelectionVisible(bool visible)
        {
            if (opponentSelectionRoot)
            {
                opponentSelectionRoot.SetActive(visible);
            }

            if (aiOpponentButton)
            {
                aiOpponentButton.interactable = visible;
            }

            if (secondPlayerButton)
            {
                secondPlayerButton.interactable = visible;
            }
        }

        private void SetSideSelectionVisible(bool visible)
        {
            if (!sideSelectionRoot) return;

            sideSelectionRoot.SetActive(visible);

            if (leftSideButton)
            {
                leftSideButton.interactable = visible;
            }

            if (rightSideButton)
            {
                rightSideButton.interactable = visible;
            }
        }
}
