using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string HighScoreKey = "HIGH_SCORE";

    [SerializeField] private int _currentScore;
    [SerializeField] private int _highScore;

    public int CurrentScore => _currentScore;
    public int HighScore => _highScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHighScore();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnReturnToMenu += HandleReturnToMenu;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnReturnToMenu -= HandleReturnToMenu;
    }

    private void Start()
    {
        NotifyScore();
        NotifyHighScore();
    }

    public void AddScore(int amount)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (amount <= 0)
            return;

        _currentScore += amount;
        GameEvents.OnScoreChanged?.Invoke(_currentScore);

        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            SaveHighScore();
            GameEvents.OnHighScoreChanged?.Invoke(_highScore);
        }
    }

    public void ResetScore()
    {
        _currentScore = 0;
        GameEvents.OnScoreChanged?.Invoke(_currentScore);
    }

    public bool TrySpendScore(int amount)
    {
        if (amount <= 0)
            return true;

        if (_currentScore < amount)
            return false;

        _currentScore -= amount;
        GameEvents.OnScoreChanged?.Invoke(_currentScore);
        return true;
    }

    private void HandleGameStarted()
    {
        ResetScore();
    }

    private void HandleGameOver()
    {
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            SaveHighScore();
            GameEvents.OnHighScoreChanged?.Invoke(_highScore);
        }
    }

    private void HandleReturnToMenu()
    {
        NotifyScore();
        NotifyHighScore();
    }

    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HighScoreKey, _highScore);
        PlayerPrefs.Save();
    }

    private void NotifyScore()
    {
        GameEvents.OnScoreChanged?.Invoke(_currentScore);
    }

    private void NotifyHighScore()
    {
        GameEvents.OnHighScoreChanged?.Invoke(_highScore);
    }
}