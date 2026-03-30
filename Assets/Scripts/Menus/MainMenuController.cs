using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _highScoreText;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
        GameEvents.OnHighScoreChanged += UpdateHighScore;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        GameEvents.OnHighScoreChanged -= UpdateHighScore;
    }

    private void Start()
    {
        HandleGameStateChanged(GameManager.Instance.CurrentState);
        UpdateHighScore(ScoreManager.Instance.HighScore);
    }

    private void HandleGameStateChanged(GameState state)
    {
        _root.SetActive(state == GameState.Menu);
    }

    private void UpdateHighScore(int highScore)
    {
        _highScoreText.text = $"Best: {highScore}";
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