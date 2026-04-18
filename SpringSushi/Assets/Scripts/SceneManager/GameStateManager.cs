using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Title,
        Playing,
        Paused,
        GameOver,
        GameClear
    }

    public GameState CurrentState { get; private set; } = GameState.Title;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //Debug.Log($"[GameState] Initialized -> {CurrentState}");
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            // Debug.Log($"[GameState] State unchanged: {CurrentState}");
            return;
        }

        GameState prevState = CurrentState;
        CurrentState = newState;

        //Debug.Log(
        //  $"[GameState] {prevState} -> {newState} " +
        //$"(Scene: {SceneManager.GetActiveScene().name}, Frame: {Time.frameCount})"
        //  );
    }

    public bool IsPaused => CurrentState == GameState.Paused;

    public void PauseGame()
    {
        if (IsPaused)
        {
            //Debug.Log("[GameState] PauseGame ignored (already paused)");
            return;
        }

        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }
}
