using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class StartGameMenuView : MenuViewBase
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button exitGameButton;

    public event Action StartGameClicked;
    public event Action ExitGameClicked;

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

    protected override void HandleAfterInitialize()
    {
        ValidateReferences();
    }

    private void AddButtonListeners()
    {
        if (startGameButton)
        {
            startGameButton.onClick.RemoveListener(HandleStartGameClicked);
            startGameButton.onClick.AddListener(HandleStartGameClicked);
        }

        if (exitGameButton)
        {
            exitGameButton.onClick.RemoveListener(HandleExitGameClicked);
            exitGameButton.onClick.AddListener(HandleExitGameClicked);
        }
    }

    private void RemoveButtonListeners()
    {
        if (startGameButton)
            startGameButton.onClick.RemoveListener(HandleStartGameClicked);

        if (exitGameButton)
            exitGameButton.onClick.RemoveListener(HandleExitGameClicked);
    }

    private void SetInteractable(bool interactable)
    {
        if (startGameButton)
            startGameButton.interactable = interactable;

        if (exitGameButton)
            exitGameButton.interactable = interactable;
    }

    private void HandleStartGameClicked()
    {
        StartGameClicked?.Invoke();
    }

    private void HandleExitGameClicked()
    {
        ExitGameClicked?.Invoke();
    }

    private void ValidateReferences()
    {
        if (!startGameButton)
            Debug.LogError($"{nameof(StartGameMenuView)} requires a start game button reference.", this);

        if (!exitGameButton)
            Debug.LogError($"{nameof(StartGameMenuView)} requires an exit game button reference.", this);
    }
}
