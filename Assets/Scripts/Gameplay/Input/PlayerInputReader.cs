using UnityEngine;
using UnityEngine.InputSystem;
using System;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action DashPressed;
    public event Action<int> AbilitySlotPressed;

    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;
    
    private InputActions inputActions;
    
    private InputAction moveActionPlayerOne;
    private InputAction dashActionPlayerOne;
    
    private InputAction moveActionPlayerTwo;
    private InputAction dashActionPlayerTwo;
    private InputAction[] abilitySlotActionsPlayerOne;
    private InputAction[] abilitySlotActionsPlayerTwo;

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
        inputActions.Gameplay.Enable();
        RefreshCurrentMoveInput();
        isInitialized = true;
    }

    public void Shutdown()
    {
        if (!isInitialized && inputActions == null) return;

        UnsubscribeMove();
        UnsubscribeDash();
        UnsubscribeAbilitySlots();
        
        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveActionPlayerOne = null;
        moveActionPlayerTwo = null;
        dashActionPlayerOne = null;
        dashActionPlayerTwo = null;
        abilitySlotActionsPlayerOne = null;
        abilitySlotActionsPlayerTwo = null;
        
        playerOneMoveInput = Vector2.zero;
        playerTwoMoveInput = Vector2.zero;
        
        isInitialized = false;
    }

    private void OnDisable()
    {
        Shutdown();
    }

    private void ConfigureActions()
    {
        UnsubscribeMove();
        UnsubscribeDash();
        UnsubscribeAbilitySlots();

        if (inputActions == null) return;

        moveActionPlayerOne = GetMoveAction(controlScheme);
        moveActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerMove
            : null;

        dashActionPlayerOne = GetDashAction(controlScheme);
        dashActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerDash
            : null;

        abilitySlotActionsPlayerOne = GetAbilitySlotActions(controlScheme);
        abilitySlotActionsPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetAbilitySlotActions(PlayerControlScheme.Arrows)
            : null;

        SubscribeMove();
        SubscribeDash();
        SubscribeAbilitySlots();
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

    private void UnsubscribeDash()
    {
        if (dashActionPlayerOne != null)
        {
            dashActionPlayerOne.performed -= OnDashPerformed;
        }

        if (dashActionPlayerTwo != null)
        {
            dashActionPlayerTwo.performed -= OnDashPerformed;
        }
    }

    private void SubscribeDash()
    {
        if (dashActionPlayerOne != null)
        {
            dashActionPlayerOne.performed += OnDashPerformed;
        }

        if (dashActionPlayerTwo != null)
        {
            dashActionPlayerTwo.performed += OnDashPerformed;
        }
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

    private InputAction GetDashAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerDash
            : inputActions.Gameplay.LeftPlayerDash;
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

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        DashPressed?.Invoke();
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

    private Vector2 ResolveCurrentMoveInput()
    {
        var move = playerOneMoveInput;
        if (moveActionPlayerTwo == null) return move;

        move += playerTwoMoveInput;
        return Vector2.ClampMagnitude(move, 1f);
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
