using System;

public static class GameEvents
{
    public static Action<GameState> OnGameStateChanged;

    public static Action OnGameStarted;
    public static Action OnGamePaused;
    public static Action OnGameResumed;
    public static Action OnGameOver;
    public static Action OnReturnToMenu;

    public static Action<int> OnScoreChanged;
    public static Action<int> OnHighScoreChanged;
    public static Action<int> OnWaveChanged;
    public static Action<int, int> OnPlayerHealthChanged;
}