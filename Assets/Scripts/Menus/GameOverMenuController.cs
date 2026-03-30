using UnityEngine;
using TMPro;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _finalScoreText;
    [SerializeField] private TMP_Text _highScoreText;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    private void HandleGameStateChanged(GameState state)
    {
        bool isGameOver = state == GameState.GameOver;
        _root.SetActive(isGameOver);

        if (!isGameOver)
            return;

        _finalScoreText.text = $"Score: {ScoreManager.Instance.CurrentScore}";
        _highScoreText.text = $"Best: {ScoreManager.Instance.HighScore}";
    }

    public void OnRestartPressed()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnMainMenuPressed()
    {
        GameManager.Instance.ReturnToMenu();
    }
}