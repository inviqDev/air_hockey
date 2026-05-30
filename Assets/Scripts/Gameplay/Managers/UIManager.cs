using System;
using UI;
using UI.Enums;
using UnityEngine;

namespace Managers
{
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private InGameUIController inGameUI;
        [SerializeField] private GameHandlerUIController gameHandlerUI;
        [SerializeField] private MatchUIView matchView;
        
        public static UIManager Instance { get; private set; }
        public event Action<MainMenuSelection> MainMenuSelectionMade;
        public event Action RestartClicked;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogError($"Multiple {nameof(UIManager)} instances in scene.", this);
                return;
            }

            Instance = this;
            ValidateReferences();
            matchView?.Initialize();
        }

        private void OnEnable()
        {
            if (mainMenu)
            {
                mainMenu.SelectionMade += HandleMainMenuSelectionMade;
            }

            if (matchView)
            {
                matchView.RestartClicked += HandleRestartClicked;
            }
        }

        private void OnDisable()
        {
            if (mainMenu)
            {
                mainMenu.SelectionMade -= HandleMainMenuSelectionMade;
            }

            if (matchView)
            {
                matchView.RestartClicked -= HandleRestartClicked;
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void OnValidate()
        {
            ValidateReferences();
        }

        public void ShowMainMenu()
        {
            gameHandlerUI?.ResetState();
            inGameUI?.HideImmediately();
            mainMenu?.Show();
        }

        public void HideMainMenu()
        {
            mainMenu?.Hide();
        }

        public void SetScores(int leftScore, int rightScore)
        {
            matchView?.SetScores(leftScore, rightScore);
        }

        public void ClearGoalInfo()
        {
            matchView?.SetGoalInfoText(string.Empty);
        }

        public void PlayGoalInfo(GoalResult result)
        {
            matchView?.SetScores(result.LeftScore, result.RightScore);
            matchView?.PlayGoalInfo(GetGoalInfoMessage(result));
        }

        private void HandleMainMenuSelectionMade(MainMenuSelection selection)
        {
            gameHandlerUI?.ResetState();
            inGameUI?.Show();
            MainMenuSelectionMade?.Invoke(selection);
        }

        private void HandleRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private static string GetGoalInfoMessage(GoalResult result)
        {
            return result.HasWinner
                ? $"{result.ScoringSide} wins"
                : $"Goal! {result.ScoringSide} scores";
        }

        private void ValidateReferences()
        {
            if (!mainMenu)
            {
                Debug.LogError($"{nameof(UIManager)} requires a MainMenuController reference.", this);
            }

            if (!inGameUI)
            {
                Debug.LogError($"{nameof(UIManager)} requires an InGameUIController reference.", this);
            }

            if (!gameHandlerUI)
            {
                Debug.LogError($"{nameof(UIManager)} requires a GameHandlerUIController reference.", this);
            }

            if (!matchView)
            {
                Debug.LogError($"{nameof(UIManager)} requires a MatchUIView reference.", this);
            }
        }
    }
}
