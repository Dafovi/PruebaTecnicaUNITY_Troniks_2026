using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintPressed { get; private set; }
    public bool ShootPressed { get; private set; }

    public bool Active { get; private set; }

    public static Action OnShootPressed;
    public static Action OnSwitchWeaponPressed;
    public static Action OnPausePressed;
    public static Action OnJumpPressed;

    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _shootAction;
    private InputAction _switchWeaponAction;
    private InputAction _pauseAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerInput = GetComponent<PlayerInput>();
        _playerInput.enabled = true;

        CacheActions();
    }

    private void OnEnable()
    {
        SubscribeActions();
        Activate();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
    }

    private void Update()
    {
        if (!Active)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            SprintPressed = false;
            ShootPressed = false;
            return;
        }

        MoveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        LookInput = _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        SprintPressed = _sprintAction != null && _sprintAction.ReadValue<float>() > 0f;
        ShootPressed = _shootAction != null && _shootAction.ReadValue<float>() > 0f;
    }

    public void Activate()
    {
        if (_playerInput.currentActionMap == null)
        {
            _playerInput.SwitchCurrentActionMap("Player");
            CacheActions();
        }

        _playerInput.currentActionMap.Enable();
        Active = true;
    }

    public void Deactivate()
    {
        if (_playerInput.currentActionMap != null)
        {
            _playerInput.currentActionMap.Disable();
        }

        Active = false;
        ClearRuntimeInputs();
    }

    public void SoftActivate()
    {
        Active = true;
    }

    public void SoftDeactivate()
    {
        Active = false;
        ClearRuntimeInputs();
    }

    private void CacheActions()
    {
        InputActionMap currentMap = _playerInput.currentActionMap;

        if (currentMap == null)
        {
            currentMap = _playerInput.actions.FindActionMap("Player", true);
        }

        _moveAction = currentMap.FindAction("Move", false);
        _lookAction = currentMap.FindAction("Look", false);
        _shootAction = currentMap.FindAction("Shoot", false);
        _switchWeaponAction = currentMap.FindAction("SwitchWeapon", false);
        _pauseAction = currentMap.FindAction("Pause", false);
        _sprintAction = currentMap.FindAction("Sprint", false);
        _jumpAction = currentMap.FindAction("Jump", false);
    }

    private void SubscribeActions()
    {
        if (_shootAction != null)
            _shootAction.performed += HandleShootPerformed;

        if (_switchWeaponAction != null)
            _switchWeaponAction.performed += HandleSwitchWeaponPerformed;

        if (_pauseAction != null)
            _pauseAction.performed += HandlePausePerformed;

        if (_jumpAction != null)
            _jumpAction.performed += HandleJumpPerformed;
    }

    private void UnsubscribeActions()
    {
        if (_shootAction != null)
            _shootAction.performed -= HandleShootPerformed;

        if (_switchWeaponAction != null)
            _switchWeaponAction.performed -= HandleSwitchWeaponPerformed;

        if (_pauseAction != null)
            _pauseAction.performed -= HandlePausePerformed;

        if (_jumpAction != null)
            _jumpAction.performed -= HandleJumpPerformed;
    }

    private void HandleShootPerformed(InputAction.CallbackContext context)
    {
        if (!Active)
            return;

        OnShootPressed?.Invoke();
    }

    private void HandleSwitchWeaponPerformed(InputAction.CallbackContext context)
    {
        if (!Active)
            return;

        OnSwitchWeaponPressed?.Invoke();
    }

    private void HandlePausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        if (!Active)
            return;

        OnJumpPressed?.Invoke();
    }

    private void ClearRuntimeInputs()
    {
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        SprintPressed = false;
        ShootPressed = false;
    }
}