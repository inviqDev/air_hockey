using System;
using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private StartGameMenu startGameMenu;
    [SerializeField] private InGameUI inGameUI;
    [SerializeField] private InGameMenuController inGameMenu;
    [SerializeField] private MatchUIView matchView;

    public InGameMenuController InGameMenu => inGameMenu;

    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private bool isInitialized;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (startGameMenu)
            startGameMenu.MatchConfigurationSelected += HandleMatchConfigurationSelected;
    }

    private void OnDisable()
    {
        if (startGameMenu)
            startGameMenu.MatchConfigurationSelected -= HandleMatchConfigurationSelected;
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void InitializeGameStart()
    {
        if (isInitialized) return;

        ValidateReferences();
        ShowInitialGameStartState();
        isInitialized = true;
    }

    public void ShowStartGameState(MatchManager matchManager)
    {
        var leftPlayerScore = matchManager ? matchManager.LeftScore : 0;
        var rightPlayerScore = matchManager ? matchManager.RightScore : 0;
        ShowStartGameState(leftPlayerScore, rightPlayerScore);
    }

    public void ShowMatchState(MatchManager matchManager)
    {
        if (matchManager)
            ResetMatchUI(matchManager);

        if (startGameMenu)
            startGameMenu.Hide();

        if (inGameUI)
            inGameUI.Show();
    }

    private void SetScores(int leftScore, int rightScore)
    {
        matchView?.SetScores(leftScore, rightScore);
    }

    public void ResetMatchUI(MatchManager matchManager)
    {
        var leftPlayerScore = matchManager.LeftScore;
        var rightPlayerScore = matchManager.RightScore;
        SetScores(leftPlayerScore, rightPlayerScore);

        ClearGoalPopUpText();
    }

    public void ClearGoalPopUpText()
    {
        matchView?.SetGoalInfoText(string.Empty);
    }

    public void PlayGoalInfo(GoalResult result)
    {
        matchView?.SetScores(result.LeftScore, result.RightScore);
        matchView?.PlayGoalInfo(GetGoalInfoMessage(result));
    }

    private void HandleMatchConfigurationSelected(MatchConfiguration configuration)
    {
        MatchConfigurationSelected?.Invoke(configuration);
    }

    private void ShowInitialGameStartState()
    {
        ShowStartGameState(0, 0);
    }

    private void ShowStartGameState(int leftScore, int rightScore)
    {
        SetScores(leftScore, rightScore);
        ClearGoalPopUpText();

        if (inGameUI)
            inGameUI.HideImmediately();

        if (startGameMenu)
            startGameMenu.Show();
    }

    private static string GetGoalInfoMessage(GoalResult result)
    {
        return result.HasWinner
            ? $"{result.ScoringSide} wins"
            : $"Goal! {result.ScoringSide} scores";
    }

    private void ValidateReferences()
    {
        if (!startGameMenu)
            Debug.LogError($"{nameof(UIManager)} requires a StartGameMenu reference.", this);

        if (!inGameUI)
            Debug.LogError($"{nameof(UIManager)} requires an InGameUI reference.", this);

        if (!inGameMenu)
            Debug.LogError($"{nameof(UIManager)} requires an InGameMenuController reference.", this);

        if (!matchView)
            Debug.LogError($"{nameof(UIManager)} requires a MatchUIView reference.", this);
    }
}
