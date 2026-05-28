using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputCommandSource : MonoBehaviour, IMovementCommandSource, InputActions.IGameplayActions
{
    private InputActions inputActions;
    private Vector2 move;
    private bool dashPressed;

    public MovementCommand ReadCommand()
    {
        var command = new MovementCommand(move, dashPressed);
        dashPressed = false;
        return command;
    }

    private void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Gameplay.SetCallbacks(this);
        inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        if (inputActions == null)
        {
            return;
        }

        inputActions.Gameplay.SetCallbacks(null);
        inputActions.Gameplay.Disable();
        inputActions.Dispose();
        inputActions = null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashPressed = true;
        }
    }
}
