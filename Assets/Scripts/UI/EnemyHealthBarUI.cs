using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBaseController _enemy;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Transform _lookAtCameraTarget;

    [Header("Settings")]
    [SerializeField] private bool _billboard = true;
    [SerializeField] private bool _hideWhenFull = false;
    [SerializeField] private bool _hideWhenDead = true;

    private Camera _mainCamera;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_enemy == null)
            _enemy = GetComponentInParent<EnemyBaseController>();

        if (_lookAtCameraTarget == null && _mainCamera != null)
            _lookAtCameraTarget = _mainCamera.transform;
    }

    private void Update()
    {
        if (_enemy == null)
            return;

        UpdateHealthBar();
        HandleBillboard();
    }

    private void UpdateHealthBar()
    {
        float healthPercent = _enemy.HealthPercent;

        if (_fillImage != null)
        {
            _fillImage.fillAmount = healthPercent;
        }

        // Opcional: ocultar barra
        if (_hideWhenDead && _enemy.IsDead)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_hideWhenFull)
        {
            gameObject.SetActive(healthPercent < 0.999f);
        }
    }

    private void HandleBillboard()
    {
        if (!_billboard || _lookAtCameraTarget == null)
            return;

        Vector3 direction = transform.position - _lookAtCameraTarget.position;
        direction.y = 0f; // opcional: solo rotación horizontal

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}