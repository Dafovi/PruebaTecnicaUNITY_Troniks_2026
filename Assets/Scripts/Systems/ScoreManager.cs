using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string HighWaveKey = "HIGH_WAVE";

    [SerializeField] private int _currentWave;
    [SerializeField] private int _highWave;

    public int CurrentWave => _currentWave;
    public int HighWave => _highWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHighWave();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnWaveChanged += HandleWaveChanged;
        GameEvents.OnReturnToMenu += HandleReturnToMenu;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnWaveChanged -= HandleWaveChanged;
        GameEvents.OnReturnToMenu -= HandleReturnToMenu;
    }

    private void Start()
    {
        NotifyHighWave();
    }

    private void HandleGameStarted()
    {
        _currentWave = 0;
    }

    private void HandleWaveChanged(int wave)
    {
        _currentWave = wave;

        if (_currentWave > _highWave)
        {
            _highWave = _currentWave;
            SaveHighWave();
            NotifyHighWave();
        }
    }

    private void HandleReturnToMenu()
    {
        NotifyHighWave();
    }

    private void LoadHighWave()
    {
        _highWave = PlayerPrefs.GetInt(HighWaveKey, 0);
    }

    private void SaveHighWave()
    {
        PlayerPrefs.SetInt(HighWaveKey, _highWave);
        PlayerPrefs.Save();
    }

    private void NotifyHighWave()
    {
        GameEvents.OnHighScoreChanged?.Invoke(_highWave);
    }
}