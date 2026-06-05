using System;
using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private MenuViewsContainer menuViewsContainer;
    [SerializeField] private StartGameMenu startGameMenu;
    [SerializeField] private InGameMenuView inGameMenuView;
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
        menuViewsContainer?.Initialize();
        ShowInitialGameStartState();
        isInitialized = true;
    }

    public void ShowStartGameState(MatchManager matchManager)
    {
        var leftPlayerScore = matchManager ? matchManager.LeftScore : 0;
        var rightPlayerScore = matchManager ? matchManager.RightScore : 0;
        ShowStartGameState(leftPlayerScore, rightPlayerScore, false);
    }

    public void ShowStartGameStateImmediately(MatchManager matchManager)
    {
        var leftPlayerScore = matchManager ? matchManager.LeftScore : 0;
        var rightPlayerScore = matchManager ? matchManager.RightScore : 0;
        ShowStartGameState(leftPlayerScore, rightPlayerScore, true);
    }

    public void ShowMatchState(MatchManager matchManager)
    {
        if (matchManager)
            ResetMatchUI(matchManager);

        if (startGameMenu)
            startGameMenu.Hide();

        if (inGameMenuView)
            inGameMenuView.Show();
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
        ShowStartGameState(0, 0, true);
    }

    private void ShowStartGameState(int leftScore, int rightScore, bool hideInGameImmediately)
    {
        SetScores(leftScore, rightScore);
        ClearGoalPopUpText();

        if (inGameMenuView)
        {
            if (hideInGameImmediately || !isInitialized)
                inGameMenuView.HideImmediately();
            else
                inGameMenuView.Hide();
        }

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

        if (!menuViewsContainer)
            Debug.LogError($"{nameof(UIManager)} requires a {nameof(MenuViewsContainer)} reference.", this);

        if (!inGameMenuView)
            Debug.LogError($"{nameof(UIManager)} requires an InGameMenuView reference.", this);

        if (!inGameMenu)
            Debug.LogError($"{nameof(UIManager)} requires an InGameMenuController reference.", this);

        if (!matchView)
            Debug.LogError($"{nameof(UIManager)} requires a MatchUIView reference.", this);
    }
}
