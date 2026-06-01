using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SideSelectionView : MenuViewBase
{
    [Header("Buttons")]
    [SerializeField] private Button leftSideButton;
    [SerializeField] private Button rightSideButton;
    [SerializeField] private Button backButton;

    public event Action<PlayerSide> SideSelected;
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

    private void SelectLeftSide()
    {
        SideSelected?.Invoke(PlayerSide.Left);
    }

    private void SelectRightSide()
    {
        SideSelected?.Invoke(PlayerSide.Right);
    }

    private void AddButtonListeners()
    {
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

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }
    }

    private void RemoveButtonListeners()
    {
        if (leftSideButton)
        {
            leftSideButton.onClick.RemoveListener(SelectLeftSide);
        }

        if (rightSideButton)
        {
            rightSideButton.onClick.RemoveListener(SelectRightSide);
        }

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
        }
    }

    private void SetInteractable(bool interactable)
    {
        if (leftSideButton)
        {
            leftSideButton.interactable = interactable;
        }

        if (rightSideButton)
        {
            rightSideButton.interactable = interactable;
        }

        if (backButton)
        {
            backButton.interactable = interactable;
        }
    }

    private void HandleBackClicked()
    {
        BackButtonClicked?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!leftSideButton)
        {
            Debug.LogError($"{nameof(SideSelectionView)} requires a left side button reference.", this);
        }

        if (!rightSideButton)
        {
            Debug.LogError($"{nameof(SideSelectionView)} requires a right side button reference.", this);
        }

        if (!backButton)
        {
            Debug.LogError($"{nameof(SideSelectionView)} requires a back button reference.", this);
        }
    }
}
