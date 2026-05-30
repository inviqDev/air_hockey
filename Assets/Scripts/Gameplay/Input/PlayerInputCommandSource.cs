using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputCommandSource : MonoBehaviour, IMovementCommandSource
{
    public enum KeyboardLayout
    {
        Wasd,
        Arrows
    }

    [SerializeField] private KeyboardLayout keyboardLayout = KeyboardLayout.Wasd;

    private InputActions inputActions;
    private InputAction moveAction;
    private InputAction dashAction;
    private bool dashPressed;

    public void SetKeyboardLayout(KeyboardLayout layout)
    {
        keyboardLayout = layout;

        if (inputActions != null)
        {
            ConfigureActions();
        }
    }

    public MovementCommand ReadCommand()
    {
        var move = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        var command = new MovementCommand(move, dashPressed);
        dashPressed = false;
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

        moveAction = null;
        dashAction = null;
        dashPressed = false;
    }

    private void ConfigureActions()
    {
        UnsubscribeDash();

        if (inputActions == null) return;

        moveAction = keyboardLayout == KeyboardLayout.Wasd
            ? inputActions.Gameplay.LeftPlayerMove
            : inputActions.Gameplay.RightPlayerMove;

        dashAction = keyboardLayout == KeyboardLayout.Wasd
            ? inputActions.Gameplay.LeftPlayerDash
            : inputActions.Gameplay.RightPlayerDash;

        dashAction.performed += OnDashPerformed;
    }

    private void UnsubscribeDash()
    {
        if (dashAction != null)
        {
            dashAction.performed -= OnDashPerformed;
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        dashPressed = true;
    }
}
