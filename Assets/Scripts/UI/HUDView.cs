using UnityEngine;
using TMPro;

public class HUDView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _healthText;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnWaveChanged += UpdateWave;
        GameEvents.OnPlayerHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnWaveChanged -= UpdateWave;
        GameEvents.OnPlayerHealthChanged -= UpdateHealth;
    }

    private void Start()
    {
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    private void HandleGameStateChanged(GameState state)
    {
        _root.SetActive(state == GameState.Playing);
    }

    private void UpdateScore(int score)
    {
        _scoreText.text = $"Score: {score}";
    }

    private void UpdateWave(int wave)
    {
        _waveText.text = $"Wave: {wave}";
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        _healthText.text = $"HP: {currentHealth}/{maxHealth}";
    }
}