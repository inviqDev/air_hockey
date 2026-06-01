using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputCommandSource : MonoBehaviour, IMovementCommandSource
{
    [SerializeField] private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;

    private InputActions inputActions;
    
    private InputAction moveActionPlayerOne;
    private InputAction dashActionPlayerOne;
    
    private InputAction moveActionPlayerTwo;
    private InputAction dashActionPlayerTwo;
    
    private bool dashPressed;

    public void SetControlScheme(PlayerControlScheme scheme)
    {
        controlScheme = scheme;

        if (inputActions != null)
        {
            ConfigureActions();
        }
    }

    public MovementCommand ReadCommand()
    {
        var move = moveActionPlayerOne?.ReadValue<Vector2>() ?? Vector2.zero;
        if (moveActionPlayerTwo != null)
        {
            move += moveActionPlayerTwo.ReadValue<Vector2>();
            move = Vector2.ClampMagnitude(move, 1f);
        }

        var command = new MovementCommand(move, dashPressed);
        dashPressed = false;
        
        Debug.Log($"{gameObject.name} - ReadCommand()");
        return command;
    }

    private void OnEnable()
    {
        inputActions = new InputActions();
        ConfigureActions();
        inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
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
        dashPressed = false;
    }

    private void ConfigureActions()
    {
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

        SubscribeDash();
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
        dashPressed = true;
    }
}
