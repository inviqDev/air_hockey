using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class OpponentSelectionView : MenuViewBase
{
    [Header("Buttons")]
    [SerializeField] private Button aiOpponentButton;
    [SerializeField] private Button secondPlayerButton;
    [SerializeField] private Button backButton;

    public event Action<PlayerTwoControlType> PlayerTwoControlTypeSelected;
    public event Action BackButtonClicked;

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

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }
    }

    private void RemoveButtonListeners()
    {
        if (aiOpponentButton)
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);

        if (secondPlayerButton)
            secondPlayerButton.onClick.RemoveListener(SelectSecondPlayer);

        if (backButton)
            backButton.onClick.RemoveListener(HandleBackClicked);
    }

    private void SetInteractable(bool interactable)
    {
        if (aiOpponentButton)
            aiOpponentButton.interactable = interactable;

        if (secondPlayerButton)
            secondPlayerButton.interactable = interactable;

        if (backButton)
            backButton.interactable = interactable;
    }

    private void HandleBackClicked()
    {
        BackButtonClicked?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!aiOpponentButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires an AI opponent button reference.", this);

        if (!secondPlayerButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires a second player button reference.", this);

        if (!backButton)
            Debug.LogError($"{nameof(OpponentSelectionView)} requires a back button reference.", this);
    }
}
