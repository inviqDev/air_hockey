using System;
using UnityEngine;

public sealed class MainMenuController : MenuViewBase
{
    [SerializeField] private OpponentSelectionView opponentSelection;
    [SerializeField] private SideSelectionView sideSelection;

    public event Action<MatchConfiguration> MatchConfigurationSelected;

    private PlayerTwoControlType selectedPlayerTwoControlType;

    protected override void Awake()
    {
        base.Awake();
        ValidateReferences();
        ShowOpponentSelection();
    }

    private void OnEnable()
    {
        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected += SelectMode;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected += SelectSide;
            sideSelection.BackButtonClicked += ShowOpponentSelection;
        }
    }

    private void OnDisable()
    {
        if (opponentSelection)
        {
            opponentSelection.PlayerTwoControlTypeSelected -= SelectMode;
        }

        if (sideSelection)
        {
            sideSelection.SideSelected -= SelectSide;
            sideSelection.BackButtonClicked -= ShowOpponentSelection;
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleBeforeShow()
    {
        ShowOpponentSelection();
    }

    private void SelectMode(PlayerTwoControlType playerTwoControlType)
    {
        selectedPlayerTwoControlType = playerTwoControlType;
        opponentSelection?.Hide();
        sideSelection?.Show();
    }

    private void ShowOpponentSelection()
    {
        sideSelection?.Hide();
        opponentSelection?.Show();
    }

    private void SelectSide(PlayerSide playerOneSide)
    {
        Hide();

        MatchConfigurationSelected?.Invoke(new MatchConfiguration(selectedPlayerTwoControlType, playerOneSide));
    }

    private void ValidateReferences()
    {
        if (!opponentSelection)
        {
            Debug.LogError($"{nameof(MainMenuController)} requires an opponent selection reference.", this);
        }

        if (!sideSelection)
        {
            Debug.LogError($"{nameof(MainMenuController)} requires a side selection reference.", this);
        }
    }
}
