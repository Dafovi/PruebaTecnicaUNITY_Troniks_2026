using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _cameraTransform;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _acceleration = 12f;
    [SerializeField] private float _airControlMultiplier = 0.5f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -25f;
    [SerializeField] private float _groundedGravity = -2f;
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private bool _canJump;

    private CharacterController _characterController;

    private Vector2 _moveInput;
    private Vector3 _currentHorizontalVelocity;
    private float _verticalVelocity;
    private bool _isSprinting;
    private bool _jumpQueued;

    public Vector2 MoveInput => _moveInput;
    public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
    public bool IsGrounded => _characterController.isGrounded;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        InputController.OnJumpPressed += QueueJump;
    }

    private void OnDisable()
    {
        InputController.OnJumpPressed -= QueueJump;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            ApplyIdleGravity();
            return;
        }

        if (InputController.Instance != null)
        {
            _moveInput = InputController.Instance.MoveInput;
            _isSprinting = InputController.Instance.SprintPressed;
        }

        HandleMovement();
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }

    public void SetSprint(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }

    public void QueueJump()
    {
        if (!_canJump)
            return;

        _jumpQueued = true;
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = GetMoveDirectionRelativeToView();
        float targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;
        Vector3 targetHorizontalVelocity = moveDirection * targetSpeed;

        float controlMultiplier = _characterController.isGrounded ? 1f : _airControlMultiplier;

        _currentHorizontalVelocity = Vector3.Lerp(
            _currentHorizontalVelocity,
            targetHorizontalVelocity,
            _acceleration * controlMultiplier * Time.deltaTime);

        HandleGravityAndJump();

        Vector3 finalVelocity = _currentHorizontalVelocity;
        finalVelocity.y = _verticalVelocity;

        _characterController.Move(finalVelocity * Time.deltaTime);
    }

    private Vector3 GetMoveDirectionRelativeToView()
    {
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * _moveInput.y + right * _moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        return moveDirection;
    }

    private void HandleGravityAndJump()
    {
        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = _groundedGravity;

            if (_jumpQueued && _canJump)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        _jumpQueued = false;
    }

    private void ApplyIdleGravity()
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = _groundedGravity;
        }
        else
        {
            _verticalVelocity += _gravity * Time.unscaledDeltaTime;
        }

        Vector3 motion = new Vector3(0f, _verticalVelocity, 0f);
        _characterController.Move(motion * Time.unscaledDeltaTime);
    }
}