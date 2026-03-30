using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _root;

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
        _root.SetActive(state == GameState.Pause);
    }

    public void OnResumePressed()
    {
        GameManager.Instance.ResumeGame();
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