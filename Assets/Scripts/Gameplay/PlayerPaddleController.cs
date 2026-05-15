using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaddleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 8f;

    [Header("Dash")]
    [SerializeField] private float _dashDistance = 1.0f;
    [SerializeField] private float _dashDuration = 0.08f;
    [SerializeField] private float _dashCooldown = 0.5f;

    [Header("Movement Bounds")]
    [SerializeField] private Vector2 _minBounds = new(-4.6f, -3.8f);
    [SerializeField] private Vector2 _maxBounds = new(-0.4f, 3.8f);

    private Rigidbody2D _rb;
    private InputActions _inputActions;
    
    private Vector2 _moveInput;
    private Vector2 _startPosition;

    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector2 _dashStartPosition;
    private Vector2 _dashTargetPosition;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;
        
        _rb ??= GetComponent<Rigidbody2D>();
        _inputActions ??= new InputActions();
        
        _startPosition = _rb.position;
    }

    private void OnEnable()
    {
        _inputActions ??= new InputActions();
        _inputActions.Enable();

        _inputActions.Gameplay.Move.performed += OnMovePerformed;
        _inputActions.Gameplay.Move.canceled += OnMoveCanceled;
        _inputActions.Gameplay.Dash.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        if (_inputActions == null) return;
        
        _inputActions.Gameplay.Move.performed -= OnMovePerformed;
        _inputActions.Gameplay.Move.canceled -= OnMoveCanceled;
        _inputActions.Gameplay.Dash.performed -= OnDashPerformed;

        _inputActions?.Disable();
    }

    private void Update()
    {
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            UpdateDash();
            return;
        }

        if (_moveInput == Vector2.zero) return;
        MoveNormally();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>().normalized;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        TryStartDash();
    }

    private void MoveNormally()
    {
        var nextPosition = _rb.position + _moveInput * (_moveSpeed * Time.fixedDeltaTime);
        nextPosition = ClampToBounds(nextPosition);

        _rb.MovePosition(nextPosition);
    }

    private void TryStartDash()
    {
        if (_isDashing || _dashCooldownTimer > 0f) return;

        _isDashing = true;
        _dashTimer = 0f;
        _dashCooldownTimer = _dashCooldown;

        _dashStartPosition = _rb.position;

        var dashDirection = Vector2.left;
        _dashTargetPosition = _dashStartPosition + dashDirection * _dashDistance;
        _dashTargetPosition = ClampToBounds(_dashTargetPosition);
    }

    private void UpdateDash()
    {
        _dashTimer += Time.fixedDeltaTime;

        var progress = _dashTimer / _dashDuration;
        progress = Mathf.Clamp01(progress);

        var nextPosition = Vector2.Lerp(_dashStartPosition, _dashTargetPosition, progress);
        _rb.MovePosition(nextPosition);

        if (progress >= 1f)
        {
            _isDashing = false;
        }
    }

    private Vector2 ClampToBounds(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, _minBounds.x, _maxBounds.x);
        position.y = Mathf.Clamp(position.y, _minBounds.y, _maxBounds.y);

        return position;
    }

    public void ResetPosition()
    {
        _isDashing = false;
        _dashTimer = 0f;
        _dashCooldownTimer = 0f;

        _rb.position = _startPosition;
    }
}