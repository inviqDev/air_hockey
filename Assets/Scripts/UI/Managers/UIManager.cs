using System;
using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private MainMenuController mainMenu;
    [SerializeField] private InGameUI inGameUI;
    [SerializeField] private InGameMenuController inGameMenu;
    [SerializeField] private MatchUIView matchView;
        
    public InGameMenuController InGameMenu => inGameMenu;
    
    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (mainMenu)
        {
            mainMenu.MatchConfigurationSelected += HandleMatchConfigurationSelected;
        }

    }

    private void OnDisable()
    {
        if (mainMenu)
        {
            mainMenu.MatchConfigurationSelected -= HandleMatchConfigurationSelected;
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

    private void HandleMatchConfigurationSelected(MatchConfiguration configuration)
    {
        inGameUI?.Show();
        MatchConfigurationSelected?.Invoke(configuration);
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
            Debug.LogError($"{nameof(UIManager)} requires an InGameUI reference.", this);
        }
        
        if (!inGameMenu)
        {
            Debug.LogError($"{nameof(UIManager)} requires an InGameMenuController reference.", this);
        }

        if (!matchView)
        {
            Debug.LogError($"{nameof(UIManager)} requires a MatchUIView reference.", this);
        }
    }
}
