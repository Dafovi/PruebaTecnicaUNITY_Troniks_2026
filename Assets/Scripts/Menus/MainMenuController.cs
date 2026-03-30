using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _highScoreText;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnHighScoreChanged += UpdateHighWave;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnHighScoreChanged -= UpdateHighWave;
    }

    private void Start()
    {
        HandleGameStateChanged(GameManager.Instance.CurrentState);
        UpdateHighWave(ScoreManager.Instance.HighWave);
    }

    private void HandleGameStateChanged(GameState state)
    {
        _root.SetActive(state == GameState.Menu);
    }

    private void UpdateHighWave(int highWave)
    {
        _highScoreText.text = $"Best Wave: {highWave}";
    }

    public void OnStartPressed()
    {
        GameManager.Instance.StartGame();
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}