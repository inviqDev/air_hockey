using UnityEngine;
using UnityEngine.InputSystem;
using System;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<int> AbilitySlotPressed;
    public event Action AbilitySelectionMenuPressed;

    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;
    
    private InputActions inputActions;
    private bool isGameplayInputEnabled;
    private bool isRoundBreakInputEnabled;
    
    private InputAction moveActionPlayerOne;
    
    private InputAction moveActionPlayerTwo;
    
    private InputAction[] abilitySlotActionsPlayerOne;
    private InputAction[] abilitySlotActionsPlayerTwo;
    
    private InputAction abilityMenuActionPlayerOne;
    private InputAction abilityMenuActionPlayerTwo;

    private Vector2 playerOneMoveInput;
    private Vector2 playerTwoMoveInput;
    
    private bool isInitialized;

    public Vector2 CurrentMoveInput => ResolveCurrentMoveInput();

    public void Initialize(PlayerControlScheme scheme)
    {
        Shutdown();

        controlScheme = scheme;
        inputActions = new InputActions();
        ConfigureActions();
        RefreshCurrentMoveInput();
        isInitialized = true;
    }

    public void Shutdown()
    {
        if (!isInitialized && inputActions == null) return;

        UnsubscribeMove();
        UnsubscribeAbilitySlots();
        UnsubscribeAbilityMenu();
        
        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.RoundBreak.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveActionPlayerOne = null;
        moveActionPlayerTwo = null;
        abilitySlotActionsPlayerOne = null;
        abilitySlotActionsPlayerTwo = null;
        abilityMenuActionPlayerOne = null;
        abilityMenuActionPlayerTwo = null;
        
        playerOneMoveInput = Vector2.zero;
        playerTwoMoveInput = Vector2.zero;
        isGameplayInputEnabled = false;
        isRoundBreakInputEnabled = false;
        
        isInitialized = false;
    }

    private void OnDisable()
    {
        Shutdown();
    }

    private void ConfigureActions()
    {
        UnsubscribeMove();
        UnsubscribeAbilitySlots();
        UnsubscribeAbilityMenu();

        if (inputActions == null) return;

        moveActionPlayerOne = GetMoveAction(controlScheme);
        moveActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerMove
            : null;

        abilitySlotActionsPlayerOne = GetAbilitySlotActions(controlScheme);
        abilitySlotActionsPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetAbilitySlotActions(PlayerControlScheme.Arrows)
            : null;

        abilityMenuActionPlayerOne = GetRoundBreakAbilityMenuAction(controlScheme);
        abilityMenuActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetRoundBreakAbilityMenuAction(PlayerControlScheme.Arrows)
            : null;

        SubscribeMove();
        SubscribeAbilitySlots();
        SubscribeAbilityMenu();
    }

    private void UnsubscribeMove()
    {
        if (moveActionPlayerOne != null)
        {
            moveActionPlayerOne.performed -= OnMovePlayerOneChanged;
            moveActionPlayerOne.canceled -= OnMovePlayerOneChanged;
        }

        if (moveActionPlayerTwo != null)
        {
            moveActionPlayerTwo.performed -= OnMovePlayerTwoChanged;
            moveActionPlayerTwo.canceled -= OnMovePlayerTwoChanged;
        }
    }

    private void SubscribeMove()
    {
        if (moveActionPlayerOne != null)
        {
            moveActionPlayerOne.performed += OnMovePlayerOneChanged;
            moveActionPlayerOne.canceled += OnMovePlayerOneChanged;
        }

        if (moveActionPlayerTwo != null)
        {
            moveActionPlayerTwo.performed += OnMovePlayerTwoChanged;
            moveActionPlayerTwo.canceled += OnMovePlayerTwoChanged;
        }
    }

    private void UnsubscribeAbilityMenu()
    {
        if (abilityMenuActionPlayerOne != null)
            abilityMenuActionPlayerOne.performed -= OnAbilityMenuPerformed;

        if (abilityMenuActionPlayerTwo != null)
            abilityMenuActionPlayerTwo.performed -= OnAbilityMenuPerformed;
    }

    private void SubscribeAbilityMenu()
    {
        if (abilityMenuActionPlayerOne != null)
            abilityMenuActionPlayerOne.performed += OnAbilityMenuPerformed;

        if (abilityMenuActionPlayerTwo != null)
            abilityMenuActionPlayerTwo.performed += OnAbilityMenuPerformed;
    }

    private void UnsubscribeAbilitySlots()
    {
        UnsubscribeAbilitySlots(abilitySlotActionsPlayerOne, HandleAbilitySlotPressedPlayerOne);
        UnsubscribeAbilitySlots(abilitySlotActionsPlayerTwo, HandleAbilitySlotPressedPlayerTwo);
    }

    private void SubscribeAbilitySlots()
    {
        SubscribeAbilitySlots(abilitySlotActionsPlayerOne, HandleAbilitySlotPressedPlayerOne);
        SubscribeAbilitySlots(abilitySlotActionsPlayerTwo, HandleAbilitySlotPressedPlayerTwo);
    }

    private InputAction GetMoveAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerMove
            : inputActions.Gameplay.LeftPlayerMove;
    }

    private InputAction[] GetAbilitySlotActions(PlayerControlScheme scheme)
    {
        if (scheme == PlayerControlScheme.Arrows)
        {
            return new[]
            {
                inputActions.Gameplay.RightPlayerAbilitySlot1,
                inputActions.Gameplay.RightPlayerAbilitySlot2,
                inputActions.Gameplay.RightPlayerAbilitySlot3,
                inputActions.Gameplay.RightPlayerAbilitySlot4
            };
        }

        return new[]
        {
            inputActions.Gameplay.LeftPlayerAbilitySlot1,
            inputActions.Gameplay.LeftPlayerAbilitySlot2,
            inputActions.Gameplay.LeftPlayerAbilitySlot3,
            inputActions.Gameplay.LeftPlayerAbilitySlot4
        };
    }

    private InputAction GetRoundBreakAbilityMenuAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.RoundBreak.RightPlayerAbilityMenu
            : inputActions.RoundBreak.LeftPlayerAbilityMenu;
    }

    private void OnAbilityMenuPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionMenuPressed?.Invoke();
    }

    private void HandleAbilitySlotPressedPlayerOne(InputAction.CallbackContext context)
    {
        NotifyAbilitySlotPressed(context, abilitySlotActionsPlayerOne);
    }

    private void HandleAbilitySlotPressedPlayerTwo(InputAction.CallbackContext context)
    {
        NotifyAbilitySlotPressed(context, abilitySlotActionsPlayerTwo);
    }

    private void OnMovePlayerOneChanged(InputAction.CallbackContext context)
    {
        playerOneMoveInput = context.ReadValue<Vector2>();
        MoveInputChanged?.Invoke(ResolveCurrentMoveInput());
    }

    private void OnMovePlayerTwoChanged(InputAction.CallbackContext context)
    {
        playerTwoMoveInput = context.ReadValue<Vector2>();
        MoveInputChanged?.Invoke(ResolveCurrentMoveInput());
    }

    private void RefreshCurrentMoveInput()
    {
        playerOneMoveInput = moveActionPlayerOne != null
            ? moveActionPlayerOne.ReadValue<Vector2>()
            : Vector2.zero;

        playerTwoMoveInput = moveActionPlayerTwo != null
            ? moveActionPlayerTwo.ReadValue<Vector2>()
            : Vector2.zero;

        MoveInputChanged?.Invoke(ResolveCurrentMoveInput());
    }

    public void SetGameplayInputEnabled(bool isEnabled)
    {
        if (inputActions == null)
        {
            isGameplayInputEnabled = false;
            return;
        }

        if (isGameplayInputEnabled == isEnabled) return;

        isGameplayInputEnabled = isEnabled;

        if (isGameplayInputEnabled)
        {
            inputActions.Gameplay.Enable();
            RefreshCurrentMoveInput();
            return;
        }

        inputActions.Gameplay.Disable();
        ClearCurrentMoveInput();
    }

    public void SetRoundBreakInputEnabled(bool isEnabled)
    {
        if (inputActions == null)
        {
            isRoundBreakInputEnabled = false;
            return;
        }

        if (isRoundBreakInputEnabled == isEnabled) return;

        isRoundBreakInputEnabled = isEnabled;

        if (isRoundBreakInputEnabled)
        {
            inputActions.RoundBreak.Enable();
            return;
        }

        inputActions.RoundBreak.Disable();
    }

    private Vector2 ResolveCurrentMoveInput()
    {
        var move = playerOneMoveInput;
        if (moveActionPlayerTwo == null) return move;

        move += playerTwoMoveInput;
        return Vector2.ClampMagnitude(move, 1f);
    }

    private void ClearCurrentMoveInput()
    {
        playerOneMoveInput = Vector2.zero;
        playerTwoMoveInput = Vector2.zero;
        MoveInputChanged?.Invoke(Vector2.zero);
    }

    private void NotifyAbilitySlotPressed(InputAction.CallbackContext context, InputAction[] slotActions)
    {
        if (slotActions == null) return;

        var slotIndex = Array.IndexOf(slotActions, context.action);
        if (slotIndex < 0) return;

        AbilitySlotPressed?.Invoke(slotIndex);
    }

    private static void SubscribeAbilitySlots(InputAction[] slotActions, Action<InputAction.CallbackContext> handler)
    {
        if (slotActions == null) return;

        foreach (var slotAction in slotActions)
        {
            if (slotAction == null) continue;
            slotAction.performed += handler;
        }
    }

    private static void UnsubscribeAbilitySlots(InputAction[] slotActions, Action<InputAction.CallbackContext> handler)
    {
        if (slotActions == null) return;

        foreach (var slotAction in slotActions)
        {
            if (slotAction == null) continue;
            slotAction.performed -= handler;
        }
    }
}
