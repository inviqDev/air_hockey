using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum PlayerInputMode
{
    Disabled,
    Gameplay,
    Intermission
}

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<int> AbilitySlotPressed;
    public event Action AbilitySelectionMenuPressed;
    public event Action AbilitySelectionPreviousPressed;
    public event Action AbilitySelectionNextPressed;

    
    private InputActions inputActions;
    
    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;
    private PlayerInputMode currentInputMode = PlayerInputMode.Disabled;
    
    private InputAction moveActionPlayerOne;
    private InputAction moveActionPlayerTwo;
    
    private InputAction[] abilitySlotActionsPlayerOne;
    private InputAction[] abilitySlotActionsPlayerTwo;
    
    private InputAction leftAbilitySelectionMenuAction;
    private InputAction abilityMenuActionPlayerTwo;
    private InputAction leftAbilitySelectionPreviousAction;
    private InputAction leftAbilitySelectionNextAction;
    private InputAction rightAbilitySelectionPreviousAction;
    private InputAction rightAbilitySelectionNextAction;

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
        UnsubscribeAbilitySelectionMenu();
        UnsubscribeAbilitySelectionNavigation();
        
        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.Intermission.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveActionPlayerOne = null;
        moveActionPlayerTwo = null;
        abilitySlotActionsPlayerOne = null;
        abilitySlotActionsPlayerTwo = null;
        leftAbilitySelectionMenuAction = null;
        abilityMenuActionPlayerTwo = null;
        leftAbilitySelectionPreviousAction = null;
        leftAbilitySelectionNextAction = null;
        rightAbilitySelectionPreviousAction = null;
        rightAbilitySelectionNextAction = null;
        
        playerOneMoveInput = Vector2.zero;
        playerTwoMoveInput = Vector2.zero;
        currentInputMode = PlayerInputMode.Disabled;
        
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
        UnsubscribeAbilitySelectionMenu();
        UnsubscribeAbilitySelectionNavigation();

        if (inputActions == null) return;

        moveActionPlayerOne = GetMoveAction(controlScheme);
        moveActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerMove
            : null;

        abilitySlotActionsPlayerOne = GetAbilitySlotActions(controlScheme);
        abilitySlotActionsPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetAbilitySlotActions(PlayerControlScheme.Arrows)
            : null;

        leftAbilitySelectionMenuAction = GetIntermissionAbilityMenuAction(controlScheme);
        abilityMenuActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetIntermissionAbilityMenuAction(PlayerControlScheme.Arrows)
            : null;
        leftAbilitySelectionPreviousAction = GetIntermissionPreviousAction(controlScheme);
        leftAbilitySelectionNextAction = GetIntermissionNextAction(controlScheme);
        rightAbilitySelectionPreviousAction = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetIntermissionPreviousAction(PlayerControlScheme.Arrows)
            : null;
        rightAbilitySelectionNextAction = controlScheme == PlayerControlScheme.WasdAndArrows
            ? GetIntermissionNextAction(PlayerControlScheme.Arrows)
            : null;

        SubscribeMove();
        SubscribeAbilitySlots();
        SubscribeAbilityMenu();
        SubscribeAbilitySelectionNavigation();
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

    private void UnsubscribeAbilitySelectionMenu()
    {
        if (leftAbilitySelectionMenuAction != null)
            leftAbilitySelectionMenuAction.performed -= OnLeftAbilitySelectionMenuPerformed;

        if (abilityMenuActionPlayerTwo != null)
            abilityMenuActionPlayerTwo.performed -= OnLeftAbilitySelectionMenuPerformed;
    }

    private void SubscribeAbilityMenu()
    {
        if (leftAbilitySelectionMenuAction != null)
            leftAbilitySelectionMenuAction.performed += OnLeftAbilitySelectionMenuPerformed;

        if (abilityMenuActionPlayerTwo != null)
            abilityMenuActionPlayerTwo.performed += OnLeftAbilitySelectionMenuPerformed;
    }

    private void UnsubscribeAbilitySelectionNavigation()
    {
        if (leftAbilitySelectionPreviousAction != null)
            leftAbilitySelectionPreviousAction.performed -= OnAbilitySelectionPreviousPerformed;

        if (leftAbilitySelectionNextAction != null)
            leftAbilitySelectionNextAction.performed -= OnAbilitySelectionNextPerformed;

        if (rightAbilitySelectionPreviousAction != null)
            rightAbilitySelectionPreviousAction.performed -= OnAbilitySelectionPreviousPerformed;

        if (rightAbilitySelectionNextAction != null)
            rightAbilitySelectionNextAction.performed -= OnAbilitySelectionNextPerformed;
    }

    private void SubscribeAbilitySelectionNavigation()
    {
        if (leftAbilitySelectionPreviousAction != null)
            leftAbilitySelectionPreviousAction.performed += OnAbilitySelectionPreviousPerformed;

        if (leftAbilitySelectionNextAction != null)
            leftAbilitySelectionNextAction.performed += OnAbilitySelectionNextPerformed;

        if (rightAbilitySelectionPreviousAction != null)
            rightAbilitySelectionPreviousAction.performed += OnAbilitySelectionPreviousPerformed;

        if (rightAbilitySelectionNextAction != null)
            rightAbilitySelectionNextAction.performed += OnAbilitySelectionNextPerformed;
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

    private InputAction GetIntermissionAbilityMenuAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Intermission.RightPlayerAbilityMenu
            : inputActions.Intermission.LeftPlayerAbilityMenu;
    }

    private InputAction GetIntermissionPreviousAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Intermission.RightPlayerPreviousOffer
            : inputActions.Intermission.LeftPlayerPreviousOffer;
    }

    private InputAction GetIntermissionNextAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Intermission.RightPlayerNextOffer
            : inputActions.Intermission.LeftPlayerNextOffer;
    }

    private void OnLeftAbilitySelectionMenuPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionMenuPressed?.Invoke();
    }

    private void OnAbilitySelectionPreviousPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionPreviousPressed?.Invoke();
    }

    private void OnAbilitySelectionNextPerformed(InputAction.CallbackContext context)
    {
        AbilitySelectionNextPressed?.Invoke();
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
        inputActions.Intermission.Disable();

        if (wasGameplayActive && currentInputMode != PlayerInputMode.Gameplay)
        {
            ClearCurrentMoveInput();
        }

        switch (currentInputMode)
        {
            case PlayerInputMode.Gameplay:
                inputActions.Gameplay.Enable();
                RefreshCurrentMoveInput();
                break;
            case PlayerInputMode.Intermission:
                inputActions.Intermission.Enable();
                break;
            case PlayerInputMode.Disabled:
            default:
                break;
        }
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
