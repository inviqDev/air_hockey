using UnityEngine;

public sealed class MatchManager : MonoBehaviour
{
    [Header("Ref Managers")]
    [SerializeField] private UIManager uiManager;

    [Header("References")]
    [SerializeField] private GoalController goalController;
    [SerializeField] private MatchModeController matchModeController;
    [SerializeField] private RoundResetter roundResetter;
    [SerializeField] private TurnFlowController turnFlow;

    public int LeftScore => goalController ? goalController.LeftScore : 0;
    public int RightScore => goalController ? goalController.RightScore : 0;
    public bool IsTurnActive => turnFlow && turnFlow.IsTurnActive;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (uiManager)
        {
            uiManager.MatchConfigurationSelected += HandleMatchConfigurationSelected;
            uiManager.RestartClicked += RestartMatch;
        }

        if (goalController)
        {
            goalController.GoalResolved += HandleGoalResult;
        }
    }

    private void OnDisable()
    {
        if (uiManager)
        {
            uiManager.MatchConfigurationSelected -= HandleMatchConfigurationSelected;
            uiManager.RestartClicked -= RestartMatch;
        }

        if (goalController)
        {
            goalController.GoalResolved -= HandleGoalResult;
        }
    }

    private void Start()
    {
        uiManager?.SetScores(LeftScore, RightScore);
        roundResetter?.DespawnGameItems();
        ShowStartGameMenu();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void RestartMatch()
    {
        goalController?.ResetMatch();
        turnFlow?.EndTurn();
        roundResetter?.DespawnGameItems();
        uiManager?.SetScores(LeftScore, RightScore);
        uiManager?.ClearGoalInfo();

        ShowStartGameMenu();
    }

    private void ShowStartGameMenu()
    {
        turnFlow?.EndTurn();
        uiManager?.ShowMainMenu();
    }

    private void HandleMatchConfigurationSelected(MatchConfiguration configuration)
    {
        matchModeController?.StartMatch(configuration);
        PrepareNextTurn(string.Empty);
    }

    private void PrepareNextTurn(string stateMessage)
    {
        turnFlow?.PrepareTurn(() => ResetRoundAndStatus(stateMessage));
    }

    private void ResetRoundAndStatus(string stateMessage)
    {
        roundResetter?.ResetRound();
        uiManager?.ClearGoalInfo();
    }

    private void HandleGoalResult(GoalResult result)
    {
        turnFlow?.EndTurn();
        uiManager?.PlayGoalInfo(result);

        if (!result.HasWinner)
        {
            turnFlow?.PrepareTurnAfterGoalDelay(() => ResetRoundAndStatus(string.Empty));
            return;
        }

        roundResetter?.DespawnGameItems();
    }

    private void ValidateReferences()
    {
        if (goalController == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a GoalController reference.", this);
        }

        if (matchModeController == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a MatchModeController reference.", this);
        }

        if (roundResetter == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a RoundResetter reference.", this);
        }

        if (turnFlow == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a TurnFlowController reference.", this);
        }

        if (uiManager == null)
        {
            Debug.LogError($"{nameof(MatchManager)} requires a UIManager reference.", this);
        }
    }
}
