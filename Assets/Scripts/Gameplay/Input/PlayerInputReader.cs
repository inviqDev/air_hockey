using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<int> AbilitySlotPressed;

    private InputActions inputActions;
    private PlayerInputMode currentInputMode = PlayerInputMode.Disabled;

    private InputAction moveAction;
    private InputAction[] abilitySlotActions;

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

        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveAction = null;
        abilitySlotActions = null;
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

        SubscribeMove();
        SubscribeAbilitySlots();
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

        if (wasGameplayActive && currentInputMode != PlayerInputMode.Gameplay)
            ClearCurrentMoveInput();

        switch (currentInputMode)
        {
            case PlayerInputMode.Gameplay:
                inputActions.Gameplay.Enable();
                RefreshCurrentMoveInput();
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
