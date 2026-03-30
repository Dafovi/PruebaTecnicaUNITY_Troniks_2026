using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState _currentState = GameState.Menu;

    public GameState CurrentState => _currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.Menu);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        SetState(GameState.Playing);
        GameEvents.OnGameStarted?.Invoke();
    }

    public void PauseGame()
    {
        if (_currentState != GameState.Playing)
            return;

        Time.timeScale = 0f;
        SetState(GameState.Pause);
        GameEvents.OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (_currentState != GameState.Pause)
            return;

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        GameEvents.OnGameResumed?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (_currentState == GameState.GameOver)
            return;

        Time.timeScale = 0f;
        SetState(GameState.GameOver);
        GameEvents.OnGameOver?.Invoke();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.Menu);
        GameEvents.OnReturnToMenu?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetState(GameState.Playing);
        GameEvents.OnGameStarted?.Invoke();
    }

    private void SetState(GameState newState)
    {
        _currentState = newState;
        GameEvents.OnGameStateChanged?.Invoke(_currentState);
    }
}