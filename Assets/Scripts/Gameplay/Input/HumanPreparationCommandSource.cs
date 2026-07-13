using System;
using UnityEngine.InputSystem;

public sealed class HumanPreparationCommandSource : IDisposable
{
    private InputActions inputActions;
    private InputAction abilityMenuAction;
    private InputAction previousOfferAction;
    private InputAction nextOfferAction;
    private InputAction confirmSelectionAction;
    private InputAction backSelectionAction;
    private InputAction readyToggleAction;

    private bool isEnabled;
    private bool isDisposed;

    public event Action AbilityMenuPressed;
    public event Action PreviousOfferPressed;
    public event Action NextOfferPressed;
    public event Action ConfirmSelectionPressed;
    public event Action BackSelectionPressed;
    public event Action ReadyToggleRequested;

    public HumanPreparationCommandSource(InputLayout inputLayout)
    {
        inputActions = new InputActions();
        inputActions.bindingMask = InputBinding.MaskByGroup(GetBindingGroup(inputLayout));

        ConfigureActions();
        SubscribeToActions();
    }

    public void Enable()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(HumanPreparationCommandSource));

        if (isEnabled) return;

        inputActions.Preparation.Enable();
        isEnabled = true;
    }

    public void Disable()
    {
        if (!isEnabled) return;

        inputActions.Preparation.Disable();
        isEnabled = false;
    }

    public void Dispose()
    {
        if (isDisposed) return;

        Disable();
        UnsubscribeFromActions();
        inputActions.Dispose();

        inputActions = null;
        abilityMenuAction = null;
        previousOfferAction = null;
        nextOfferAction = null;
        confirmSelectionAction = null;
        backSelectionAction = null;
        readyToggleAction = null;
        isDisposed = true;
    }

    private void ConfigureActions()
    {
        abilityMenuAction = inputActions.Preparation.AbilityMenu;
        previousOfferAction = inputActions.Preparation.PreviousOffer;
        nextOfferAction = inputActions.Preparation.NextOffer;
        confirmSelectionAction = inputActions.Preparation.ConfirmSelection;
        backSelectionAction = inputActions.Preparation.BackSelection;
        readyToggleAction = inputActions.Preparation.ReadyToggle;
    }

    private void SubscribeToActions()
    {
        abilityMenuAction.performed += HandleAbilityMenuPerformed;
        previousOfferAction.performed += HandlePreviousOfferPerformed;
        nextOfferAction.performed += HandleNextOfferPerformed;
        confirmSelectionAction.performed += HandleConfirmSelectionPerformed;
        backSelectionAction.performed += HandleBackSelectionPerformed;
        readyToggleAction.performed += HandleReadyTogglePerformed;
    }

    private void UnsubscribeFromActions()
    {
        abilityMenuAction.performed -= HandleAbilityMenuPerformed;
        previousOfferAction.performed -= HandlePreviousOfferPerformed;
        nextOfferAction.performed -= HandleNextOfferPerformed;
        confirmSelectionAction.performed -= HandleConfirmSelectionPerformed;
        backSelectionAction.performed -= HandleBackSelectionPerformed;
        readyToggleAction.performed -= HandleReadyTogglePerformed;
    }

    private void HandleAbilityMenuPerformed(InputAction.CallbackContext context)
    {
        AbilityMenuPressed?.Invoke();
    }

    private void HandlePreviousOfferPerformed(InputAction.CallbackContext context)
    {
        PreviousOfferPressed?.Invoke();
    }

    private void HandleNextOfferPerformed(InputAction.CallbackContext context)
    {
        NextOfferPressed?.Invoke();
    }

    private void HandleConfirmSelectionPerformed(InputAction.CallbackContext context)
    {
        ConfirmSelectionPressed?.Invoke();
    }

    private void HandleBackSelectionPerformed(InputAction.CallbackContext context)
    {
        BackSelectionPressed?.Invoke();
    }

    private void HandleReadyTogglePerformed(InputAction.CallbackContext context)
    {
        ReadyToggleRequested?.Invoke();
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
}
