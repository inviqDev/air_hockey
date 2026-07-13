using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<int> AbilitySlotPressed;
    public event Action AbilitySelectionMenuPressed;
    public event Action AbilitySelectionPreviousPressed;
    public event Action AbilitySelectionNextPressed;
    public event Action AbilitySelectionConfirmPressed;
    public event Action AbilitySelectionBackPressed;
    public event Action ReadyToggleRequested;

    private InputActions inputActions;
    private PlayerInputMode currentInputMode = PlayerInputMode.Disabled;

    private InputAction moveAction;
    private InputAction[] abilitySlotActions;
    private InputAction abilitySelectionMenuAction;
    private InputAction abilitySelectionPreviousAction;
    private InputAction abilitySelectionNextAction;
    private InputAction abilitySelectionConfirmAction;
    private InputAction abilitySelectionBackAction;
    private InputAction readyToggleAction;

    private Vector2 currentMoveInput;
    private bool isInitialized;

    public Vector2 CurrentMoveInput => currentMoveInput;

    public void Initialize(InputLayout layout)
    {
        Shutdown();

        inputActions = new InputActions();
        inputActions.bindingMask = InputBinding.MaskByGroup(GetBindingGroup(inputActions, layout));

        ConfigureActions();
        RefreshCurrentMoveInput();

        isInitialized = true;
    }

    public void Shutdown()
    {
        if (!isInitialized && inputActions == null) return;

        UnsubscribeMove();
        UnsubscribeAbilitySlots();
        UnsubscribeAbilitySelectionMenu();
        UnsubscribeAbilitySelectionNavigation();
        UnsubscribeAbilitySelectionDecision();

        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.Preparation.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveAction = null;
        abilitySlotActions = null;
        abilitySelectionMenuAction = null;
        abilitySelectionPreviousAction = null;
        abilitySelectionNextAction = null;
        abilitySelectionConfirmAction = null;
        abilitySelectionBackAction = null;
        readyToggleAction = null;
        currentMoveInput = Vector2.zero;
        currentInputMode = PlayerInputMode.Disabled;
        isInitialized = false;
    }

    private void OnDisable()
    {
        Shutdown();
    }

    private void ConfigureActions()
    {
        if (inputActions == null) return;

        moveAction = inputActions.Gameplay.Move;

        abilitySlotActions = new[]
        {
            inputActions.Gameplay.AbilitySlot1,
            inputActions.Gameplay.AbilitySlot2,
            inputActions.Gameplay.AbilitySlot3,
            inputActions.Gameplay.AbilitySlot4
        };

        abilitySelectionMenuAction = inputActions.Preparation.AbilityMenu;
        abilitySelectionPreviousAction = inputActions.Preparation.PreviousOffer;
        abilitySelectionNextAction = inputActions.Preparation.NextOffer;
        abilitySelectionConfirmAction = inputActions.Preparation.ConfirmSelection;
        abilitySelectionBackAction = inputActions.Preparation.BackSelection;
        readyToggleAction = inputActions.Preparation.ReadyToggle;

        SubscribeMove();
        SubscribeAbilitySlots();
        SubscribeAbilitySelectionMenu();
        SubscribeAbilitySelectionNavigation();
        SubscribeAbilitySelectionDecision();
    }

    private void SubscribeMove()
    {
        if (moveAction == null) return;

        moveAction.performed += HandleMoveChanged;
        moveAction.canceled += HandleMoveChanged;
    }

    private void UnsubscribeMove()
    {
        if (moveAction == null) return;

        moveAction.performed -= HandleMoveChanged;
        moveAction.canceled -= HandleMoveChanged;
    }

    private void SubscribeAbilitySlots()
    {
        if (abilitySlotActions == null) return;

        foreach (var abilitySlotAction in abilitySlotActions)
            abilitySlotAction.performed += HandleAbilitySlotPressed;
    }

    private void UnsubscribeAbilitySlots()
    {
        if (abilitySlotActions == null) return;

        foreach (var abilitySlotAction in abilitySlotActions)
            abilitySlotAction.performed -= HandleAbilitySlotPressed;
    }

    private void SubscribeAbilitySelectionMenu()
    {
        if (abilitySelectionMenuAction != null)
            abilitySelectionMenuAction.performed += HandleAbilitySelectionMenuPerformed;
    }

    private void UnsubscribeAbilitySelectionMenu()
    {
        if (abilitySelectionMenuAction != null)
            abilitySelectionMenuAction.performed -= HandleAbilitySelectionMenuPerformed;
    }

    private void SubscribeAbilitySelectionNavigation()
    {
        if (abilitySelectionPreviousAction != null)
            abilitySelectionPreviousAction.performed += HandleAbilitySelectionPreviousPerformed;

        if (abilitySelectionNextAction != null)
            abilitySelectionNextAction.performed += HandleAbilitySelectionNextPerformed;
    }

    private void UnsubscribeAbilitySelectionNavigation()
    {
        if (abilitySelectionPreviousAction != null)
            abilitySelectionPreviousAction.performed -= HandleAbilitySelectionPreviousPerformed;

        if (abilitySelectionNextAction != null)
            abilitySelectionNextAction.performed -= HandleAbilitySelectionNextPerformed;
    }

    private void SubscribeAbilitySelectionDecision()
    {
        if (abilitySelectionConfirmAction != null)
            abilitySelectionConfirmAction.performed += HandleAbilitySelectionConfirmPerformed;

        if (abilitySelectionBackAction != null)
            abilitySelectionBackAction.performed += HandleAbilitySelectionBackPerformed;

        if (readyToggleAction != null)
            readyToggleAction.performed += HandleReadyTogglePerformed;
    }

    private void UnsubscribeAbilitySelectionDecision()
    {
        if (abilitySelectionConfirmAction != null)
            abilitySelectionConfirmAction.performed -= HandleAbilitySelectionConfirmPerformed;

        if (abilitySelectionBackAction != null)
            abilitySelectionBackAction.performed -= HandleAbilitySelectionBackPerformed;

        if (readyToggleAction != null)
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

    private void HandleAbilitySelectionMenuPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionMenuPressed?.Invoke();
    }

    private void HandleAbilitySelectionPreviousPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionPreviousPressed?.Invoke();
    }

    private void HandleAbilitySelectionNextPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionNextPressed?.Invoke();
    }

    private void HandleAbilitySelectionConfirmPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionConfirmPressed?.Invoke();
    }

    private void HandleAbilitySelectionBackPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionBackPressed?.Invoke();
    }

    private void HandleReadyTogglePerformed(InputAction.CallbackContext context)
    {
        ReadyToggleRequested?.Invoke();
    }

    private void RefreshCurrentMoveInput()
    {
        currentMoveInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        MoveInputChanged?.Invoke(currentMoveInput);
    }

    public void SetInputMode(PlayerInputMode inputMode)
    {
        if (inputActions == null)
        {
            currentInputMode = PlayerInputMode.Disabled;
            ClearCurrentMoveInput();
            return;
        }

        if (currentInputMode == inputMode) return;

        var wasGameplayActive = currentInputMode == PlayerInputMode.Gameplay;
        currentInputMode = inputMode;

        inputActions.Gameplay.Disable();
        inputActions.Preparation.Disable();

        if (wasGameplayActive && currentInputMode != PlayerInputMode.Gameplay)
            ClearCurrentMoveInput();

        switch (currentInputMode)
        {
            case PlayerInputMode.Gameplay:
                inputActions.Gameplay.Enable();
                RefreshCurrentMoveInput();
                break;
            case PlayerInputMode.Preparation:
                inputActions.Preparation.Enable();
                break;
        }
    }

    private static string GetBindingGroup(InputActions actions, InputLayout layout)
    {
        return layout switch
        {
            InputLayout.Wasd => actions.KeyboardWASDScheme.bindingGroup,
            InputLayout.Arrows => actions.KeyboardArrowsScheme.bindingGroup,
            _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, "Unsupported input layout.")
        };
    }

    private void ClearCurrentMoveInput()
    {
        currentMoveInput = Vector2.zero;
        MoveInputChanged?.Invoke(currentMoveInput);
    }
}
