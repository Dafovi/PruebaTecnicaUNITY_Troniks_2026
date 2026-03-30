using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private void OnEnable()
    {
        InputController.OnPausePressed += TogglePause;
    }

    private void OnDisable()
    {
        InputController.OnPausePressed -= TogglePause;
    }

    public void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
            return;
        }

        if (GameManager.Instance.CurrentState == GameState.Pause)
        {
            GameManager.Instance.ResumeGame();
        }
    }
}