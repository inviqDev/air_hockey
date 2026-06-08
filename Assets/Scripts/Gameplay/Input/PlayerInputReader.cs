using UnityEngine;
using UnityEngine.InputSystem;
using System;

public sealed class PlayerInputCommandSource : MonoBehaviour
{
    public event Action<Vector2> MoveInputChanged;
    public event Action<bool> DashInputChanged;

    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;
    
    private InputActions inputActions;
    
    private InputAction moveActionPlayerOne;
    private InputAction dashActionPlayerOne;
    
    private InputAction moveActionPlayerTwo;
    private InputAction dashActionPlayerTwo;

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

        if (inputActions == null) return;

        moveActionPlayerOne = GetMoveAction(controlScheme);
        moveActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerMove
            : null;

        dashActionPlayerOne = GetDashAction(controlScheme);
        dashActionPlayerTwo = controlScheme == PlayerControlScheme.WasdAndArrows
            ? inputActions.Gameplay.RightPlayerDash
            : null;

        SubscribeMove();
        SubscribeDash();
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

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        DashInputChanged?.Invoke(true);
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
