using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class OpponentSelectionView : MenuViewBase
{
    [Header("Buttons")]
    [SerializeField] private Button aiOpponentButton;
    [SerializeField] private Button secondPlayerButton;

    public event Action<PlayerTwoControlType> PlayerTwoControlTypeSelected;

    protected override void Awake()
    {
        base.Awake();
        ValidateReferences();
    }

    private void OnEnable()
    {
        AddButtonListeners();
        SetInteractable(true);
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
        SetInteractable(false);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    protected override void HandleBeforeShow()
    {
        SetInteractable(true);
    }

    protected override void HandleBeforeHide()
    {
        SetInteractable(false);
    }

    private void SelectAiOpponent()
    {
        PlayerTwoControlTypeSelected?.Invoke(PlayerTwoControlType.Ai);
    }

    private void SelectSecondPlayer()
    {
        PlayerTwoControlTypeSelected?.Invoke(PlayerTwoControlType.Human);
    }

    private void AddButtonListeners()
    {
        if (aiOpponentButton)
        {
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
            aiOpponentButton.onClick.AddListener(SelectAiOpponent);
        }

        if (secondPlayerButton)
        {
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
            secondPlayerButton.onClick.AddListener(SelectSecondPlayer);
        }
    }

    private void RemoveButtonListeners()
    {
        if (aiOpponentButton)
        {
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
        }

        if (secondPlayerButton)
        {
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);
        }
    }

    private void SetInteractable(bool interactable)
    {
        if (aiOpponentButton)
        {
            aiOpponentButton.interactable = interactable;
        }

        if (secondPlayerButton)
        {
            secondPlayerButton.interactable = interactable;
        }
    }

    private void ValidateReferences()
    {
        if (!aiOpponentButton)
        {
            Debug.LogError($"{nameof(OpponentSelectionView)} requires an AI opponent button reference.", this);
        }

        if (!secondPlayerButton)
        {
            Debug.LogError($"{nameof(OpponentSelectionView)} requires a second player button reference.", this);
        }
    }
}
