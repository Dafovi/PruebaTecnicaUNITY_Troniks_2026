using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerBody;
    [SerializeField] private Transform _cameraRoot;

    [Header("Look Settings")]
    [SerializeField] private float _lookSensitivity = 1.2f;
    [SerializeField] private float _maxLookUpAngle = 80f;
    [SerializeField] private float _maxLookDownAngle = -80f;
    [SerializeField] private bool _invertY;

    private Vector2 _lookInput;
    private float _cameraPitch;

    private void Start()
    {
        RefreshCursorState();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (InputController.Instance != null)
        {
            _lookInput = InputController.Instance.LookInput;
        }

        HandleLook();
    }

    public void SetLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }

    private void HandleLook()
    {
        float mouseX = _lookInput.x * _lookSensitivity;
        float mouseY = _lookInput.y * _lookSensitivity * (_invertY ? 1f : -1f);

        _playerBody.Rotate(Vector3.up * mouseX);

        _cameraPitch += mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _maxLookDownAngle, _maxLookUpAngle);

        _cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void HandleGameStateChanged(GameState state)
    {
        RefreshCursorState();
    }

    private void RefreshCursorState()
    {
        bool shouldLockCursor = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;

        Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLockCursor;
    }
}