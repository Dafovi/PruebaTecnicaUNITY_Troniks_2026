using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDView : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Optional Old Score")]
    [SerializeField] private TMP_Text _scoreText;

    [Header("Wave")]
    [SerializeField] private TMP_Text _waveText;

    [Header("Health")]
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private TMP_Text _healthText;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnWaveChanged += UpdateWave;
        GameEvents.OnPlayerHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnWaveChanged -= UpdateWave;
        GameEvents.OnPlayerHealthChanged -= UpdateHealth;
    }

    private void Start()
    {
        HandleGameStateChanged(GameManager.Instance.CurrentState);

        if (_scoreText != null)
        {
            _scoreText.gameObject.SetActive(false);
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        _root.SetActive(state == GameState.Playing);
    }

    private void UpdateWave(int wave)
    {
        if (_waveText != null)
        {
            _waveText.text = $"Wave: {wave}";
        }
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_healthText != null)
        {
            _healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }

        if (_healthFillImage != null)
        {
            float fill = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            _healthFillImage.fillAmount = fill;
        }
    }
}