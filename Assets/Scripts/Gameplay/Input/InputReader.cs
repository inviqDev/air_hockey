using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputReader : IDisposable
{
    private InputActions inputActions;
    private InputAction moveAction;
    private InputAction[] abilitySlotActions;
    private InputAction abilityMenuAction;
    private InputAction previousOfferAction;
    private InputAction nextOfferAction;
    private InputAction confirmSelectionAction;
    private InputAction backSelectionAction;
    private InputAction readyToggleAction;

    private PlayerInputMode currentInputMode = PlayerInputMode.Disabled;
    private Vector2 currentMoveInput;
    private bool isDisposed;

    public event Action<Vector2> MoveInputChanged;
    public event Action<int> AbilitySlotPressed;
    public event Action AbilityMenuPressed;
    public event Action PreviousOfferPressed;
    public event Action NextOfferPressed;
    public event Action ConfirmSelectionPressed;
    public event Action BackSelectionPressed;
    public event Action ReadyToggleRequested;

    public InputReader(InputLayout inputLayout)
    {
        inputActions = new InputActions();
        inputActions.bindingMask = InputBinding.MaskByGroup(GetBindingGroup(inputLayout));

        ConfigureActions();
        SubscribeToActions();
    }

    public void ApplyInputMode(PlayerInputMode inputMode)
    {
        ThrowIfDisposed();

        if (currentInputMode == inputMode) return;

        switch (inputMode)
        {
            case PlayerInputMode.Disabled:
            case PlayerInputMode.Gameplay:
            case PlayerInputMode.Preparation:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputMode), inputMode, "Unsupported input mode.");
        }

        currentInputMode = inputMode;
        inputActions.Gameplay.Disable();
        inputActions.Preparation.Disable();

        switch (inputMode)
        {
            case PlayerInputMode.Disabled:
                ClearCurrentMoveInput();
                break;
            case PlayerInputMode.Gameplay:
                inputActions.Gameplay.Enable();
                RefreshMoveInput();
                break;
            case PlayerInputMode.Preparation:
                ClearCurrentMoveInput();
                inputActions.Preparation.Enable();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputMode), inputMode, "Unsupported input mode.");
        }
    }

    public void RefreshMoveInput()
    {
        ThrowIfDisposed();

        if (currentInputMode != PlayerInputMode.Gameplay) return;

        currentMoveInput = moveAction.ReadValue<Vector2>();
        MoveInputChanged?.Invoke(currentMoveInput);
    }

    public void Dispose()
    {
        if (isDisposed) return;

        inputActions.Gameplay.Disable();
        inputActions.Preparation.Disable();
        ClearCurrentMoveInput();
        UnsubscribeFromActions();
        inputActions.Dispose();

        inputActions = null;
        moveAction = null;
        abilitySlotActions = null;
        abilityMenuAction = null;
        previousOfferAction = null;
        nextOfferAction = null;
        confirmSelectionAction = null;
        backSelectionAction = null;
        readyToggleAction = null;
        currentInputMode = PlayerInputMode.Disabled;
        isDisposed = true;
    }

    private void ConfigureActions()
    {
        moveAction = inputActions.Gameplay.Move;
        abilitySlotActions = new[]
        {
            inputActions.Gameplay.AbilitySlot1,
            inputActions.Gameplay.AbilitySlot2,
            inputActions.Gameplay.AbilitySlot3,
            inputActions.Gameplay.AbilitySlot4
        };

        abilityMenuAction = inputActions.Preparation.AbilityMenu;
        previousOfferAction = inputActions.Preparation.PreviousOffer;
        nextOfferAction = inputActions.Preparation.NextOffer;
        confirmSelectionAction = inputActions.Preparation.ConfirmSelection;
        backSelectionAction = inputActions.Preparation.BackSelection;
        readyToggleAction = inputActions.Preparation.ReadyToggle;
    }

    private void SubscribeToActions()
    {
        moveAction.performed += HandleMoveChanged;
        moveAction.canceled += HandleMoveChanged;

        foreach (var abilitySlotAction in abilitySlotActions)
            abilitySlotAction.performed += HandleAbilitySlotPressed;

        abilityMenuAction.performed += HandleAbilityMenuPerformed;
        previousOfferAction.performed += HandlePreviousOfferPerformed;
        nextOfferAction.performed += HandleNextOfferPerformed;
        confirmSelectionAction.performed += HandleConfirmSelectionPerformed;
        backSelectionAction.performed += HandleBackSelectionPerformed;
        readyToggleAction.performed += HandleReadyTogglePerformed;
    }

    private void UnsubscribeFromActions()
    {
        moveAction.performed -= HandleMoveChanged;
        moveAction.canceled -= HandleMoveChanged;

        foreach (var abilitySlotAction in abilitySlotActions)
            abilitySlotAction.performed -= HandleAbilitySlotPressed;

        abilityMenuAction.performed -= HandleAbilityMenuPerformed;
        previousOfferAction.performed -= HandlePreviousOfferPerformed;
        nextOfferAction.performed -= HandleNextOfferPerformed;
        confirmSelectionAction.performed -= HandleConfirmSelectionPerformed;
        backSelectionAction.performed -= HandleBackSelectionPerformed;
        readyToggleAction.performed -= HandleReadyTogglePerformed;
    }

    private void HandleMoveChanged(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        MoveInputChanged?.Invoke(currentMoveInput);
    }

    private void HandleAbilitySlotPressed(InputAction.CallbackContext context)
    {
        var slotIndex = Array.IndexOf(abilitySlotActions, context.action);
        if (slotIndex >= 0)
            AbilitySlotPressed?.Invoke(slotIndex);
    }

    private void HandleAbilityMenuPerformed(InputAction.CallbackContext context) => AbilityMenuPressed?.Invoke();
    private void HandlePreviousOfferPerformed(InputAction.CallbackContext context) => PreviousOfferPressed?.Invoke();
    private void HandleNextOfferPerformed(InputAction.CallbackContext context) => NextOfferPressed?.Invoke();
    private void HandleConfirmSelectionPerformed(InputAction.CallbackContext context) => ConfirmSelectionPressed?.Invoke();
    private void HandleBackSelectionPerformed(InputAction.CallbackContext context) => BackSelectionPressed?.Invoke();
    private void HandleReadyTogglePerformed(InputAction.CallbackContext context) => ReadyToggleRequested?.Invoke();

    private void ClearCurrentMoveInput()
    {
        currentMoveInput = Vector2.zero;
        MoveInputChanged?.Invoke(currentMoveInput);
    }

    private string GetBindingGroup(InputLayout inputLayout)
    {
        return inputLayout switch
        {
            InputLayout.Wasd => inputActions.KeyboardWASDScheme.bindingGroup,
            InputLayout.Arrows => inputActions.KeyboardArrowsScheme.bindingGroup,
            _ => throw new ArgumentOutOfRangeException(nameof(inputLayout), inputLayout, "Unsupported input layout.")
        };
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(InputReader));
    }
}
