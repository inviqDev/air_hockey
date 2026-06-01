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

    private InGameMenuController inGameMenu;
    private MatchConfiguration currentConfiguration;
    private bool hasCurrentConfiguration;

    private void Awake()
    {
        ValidateReferences();

        if (uiManager && uiManager.InGameMenu)
        {
            inGameMenu = uiManager.InGameMenu;
        }
    }

    private void OnEnable()
    {
        if (uiManager)
        {
            uiManager.MatchConfigurationSelected += HandleMatchConfigurationSelected;
        }

        if (inGameMenu)
        {
            inGameMenu.RestartClicked += RestartMatch;
            inGameMenu.MainMenuClicked += ReturnToMainMenu;
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
        }

        if (inGameMenu)
        {
            inGameMenu.RestartClicked -= RestartMatch;
            inGameMenu.MainMenuClicked -= ReturnToMainMenu;
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
        if (!hasCurrentConfiguration)
        {
            return;
        }

        goalController?.ResetMatch();
        turnFlow?.EndTurn();
        roundResetter?.DespawnGameItems();
        matchModeController?.StartMatch(currentConfiguration);
        uiManager?.SetScores(LeftScore, RightScore);
        uiManager?.ClearGoalInfo();
        PrepareNextTurn();
    }

    private void ReturnToMainMenu()
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
        currentConfiguration = configuration;
        hasCurrentConfiguration = true;
        matchModeController?.StartMatch(configuration);
        PrepareNextTurn();
    }

    private void PrepareNextTurn()
    {
        turnFlow?.PrepareTurn(ResetRoundAndStatus);
    }

    private void ResetRoundAndStatus()
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
            turnFlow?.PrepareTurnAfterGoalDelay(ResetRoundAndStatus);
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
