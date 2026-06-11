using UnityEngine;
using UnityEngine.InputSystem;
using System;

public sealed class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<AbilityActivationTrigger> AbilityActivationPressed;

    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;
    
    private InputActions inputActions;
    
    private InputAction moveActionPlayerOne;
    private InputAction slotOneActionPlayerOne;
    private InputAction abilityOneActionPlayerOne;
    private InputAction abilityTwoActionPlayerOne;
    
    private InputAction moveActionPlayerTwo;
    private InputAction slotOneActionPlayerTwo;
    private InputAction abilityOneActionPlayerTwo;
    private InputAction abilityTwoActionPlayerTwo;

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
        UnsubscribeAbilityActions();
        
        if (inputActions != null)
        {
            inputActions.Gameplay.Disable();
            inputActions.Dispose();
            inputActions = null;
        }

        moveActionPlayerOne = null;
        moveActionPlayerTwo = null;
        slotOneActionPlayerOne = null;
        slotOneActionPlayerTwo = null;
        abilityOneActionPlayerOne = null;
        abilityOneActionPlayerTwo = null;
        abilityTwoActionPlayerOne = null;
        abilityTwoActionPlayerTwo = null;
        
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
        UnsubscribeAbilityActions();

        if (inputActions == null) return;

        moveActionPlayerOne = GetMoveAction(controlScheme);
        moveActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerMove
            : null;

        slotOneActionPlayerOne = GetSlotOneAction(controlScheme);
        abilityOneActionPlayerOne = GetAbilityOneAction(controlScheme);
        abilityTwoActionPlayerOne = GetAbilityTwoAction(controlScheme);
        slotOneActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerDash
            : null;
        abilityOneActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerAbilityOne
            : null;
        abilityTwoActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerAbilityTwo
            : null;

        SubscribeMove();
        SubscribeAbilityActions();
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

    private void UnsubscribeAbilityActions()
    {
        if (slotOneActionPlayerOne != null)
        {
            slotOneActionPlayerOne.performed -= OnSlotOnePerformed;
        }

        if (slotOneActionPlayerTwo != null)
        {
            slotOneActionPlayerTwo.performed -= OnSlotOnePerformed;
        }

        if (abilityOneActionPlayerOne != null)
        {
            abilityOneActionPlayerOne.performed -= OnAbilityOnePerformed;
        }

        if (abilityOneActionPlayerTwo != null)
        {
            abilityOneActionPlayerTwo.performed -= OnAbilityOnePerformed;
        }

        if (abilityTwoActionPlayerOne != null)
        {
            abilityTwoActionPlayerOne.performed -= OnAbilityTwoPerformed;
        }

        if (abilityTwoActionPlayerTwo != null)
        {
            abilityTwoActionPlayerTwo.performed -= OnAbilityTwoPerformed;
        }
    }

    private void SubscribeAbilityActions()
    {
        if (slotOneActionPlayerOne != null)
        {
            slotOneActionPlayerOne.performed += OnSlotOnePerformed;
        }

        if (slotOneActionPlayerTwo != null)
        {
            slotOneActionPlayerTwo.performed += OnSlotOnePerformed;
        }

        if (abilityOneActionPlayerOne != null)
        {
            abilityOneActionPlayerOne.performed += OnAbilityOnePerformed;
        }

        if (abilityOneActionPlayerTwo != null)
        {
            abilityOneActionPlayerTwo.performed += OnAbilityOnePerformed;
        }

        if (abilityTwoActionPlayerOne != null)
        {
            abilityTwoActionPlayerOne.performed += OnAbilityTwoPerformed;
        }

        if (abilityTwoActionPlayerTwo != null)
        {
            abilityTwoActionPlayerTwo.performed += OnAbilityTwoPerformed;
        }
    }

    private InputAction GetMoveAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerMove
            : inputActions.Gameplay.LeftPlayerMove;
    }

    private InputAction GetSlotOneAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerDash
            : inputActions.Gameplay.LeftPlayerDash;
    }

    private InputAction GetAbilityOneAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerAbilityOne
            : inputActions.Gameplay.LeftPlayerAbilityOne;
    }

    private InputAction GetAbilityTwoAction(PlayerControlScheme scheme)
    {
        return scheme == PlayerControlScheme.Arrows
            ? inputActions.Gameplay.RightPlayerAbilityTwo
            : inputActions.Gameplay.LeftPlayerAbilityTwo;
    }

    private void OnSlotOnePerformed(InputAction.CallbackContext context)
    {
        AbilityActivationPressed?.Invoke(AbilityActivationTrigger.SlotOne);
    }

    private void OnAbilityOnePerformed(InputAction.CallbackContext context)
    {
        AbilityActivationPressed?.Invoke(AbilityActivationTrigger.SlotTwo);
    }

    private void OnAbilityTwoPerformed(InputAction.CallbackContext context)
    {
        AbilityActivationPressed?.Invoke(AbilityActivationTrigger.SlotThree);
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
}
