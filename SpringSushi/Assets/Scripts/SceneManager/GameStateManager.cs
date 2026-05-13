using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Title,
        Ready,
        Playing,
        Paused,
        Result,
    }

    public GameState CurrentState { get; private set; } = GameState.Title;

    public event System.Action<GameState, GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[GameState] Initialized → {CurrentState}");
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.Log($"[GameState] SetState無視（同じState）: {CurrentState}");
            return;
        }

        // Ready中はPauseへの遷移を禁止
        if (CurrentState == GameState.Ready && newState == GameState.Paused)
        {
            Debug.LogWarning($"[GameState] ★ Ready中のPause試行をブロック (呼び出し元: {GetCallerInfo()})");
            return;
        }

        GameState prev = CurrentState;
        CurrentState = newState;
        Debug.Log($"[GameState] {prev} → {newState} (Frame:{Time.frameCount})");
        OnStateChanged?.Invoke(prev, newState);
    }

    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsReady => CurrentState == GameState.Ready;

    public void EnterReady()
    {
        Debug.Log($"[GameState] EnterReady() 呼び出し (現在:{CurrentState})");
        SetState(GameState.Ready);
    }

    public void StartPlaying()
    {
        Debug.Log($"[GameState] StartPlaying() 呼び出し (現在:{CurrentState})");
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        Debug.Log($"[GameState] PauseGame() 呼び出し (現在:{CurrentState}, 呼び出し元:{GetCallerInfo()})");

        if (IsReady)
        {
            // Ready中はStateをPausedにしない（timeScaleはInGameSequenceManagerが直接管理）
            Debug.LogWarning($"[GameState] ★ Ready中のためPause State遷移拒否");
            return;
        }
        if (IsPaused)
        {
            Debug.Log($"[GameState] すでにPaused のためスキップ");
            return;
        }

        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Debug.Log($"[GameState] ResumeGame() 呼び出し (現在:{CurrentState})");
        if (!IsPaused)
        {
            Debug.Log($"[GameState] Paused状態でないためスキップ");
            return;
        }
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void EnterResult()
    {
        Debug.Log($"[GameState] EnterResult() 呼び出し (現在:{CurrentState})");
        Time.timeScale = 1f;
        SetState(GameState.Result);
    }

    /// <summary>呼び出し元のクラス名を取得（デバッグ用）</summary>
    private string GetCallerInfo()
    {
        var trace = new System.Diagnostics.StackTrace();
        // 0=GetCallerInfo, 1=PauseGame/SetState, 2=実際の呼び出し元
        if (trace.FrameCount > 2)
        {
            var frame = trace.GetFrame(2);
            var method = frame?.GetMethod();
            return $"{method?.DeclaringType?.Name}.{method?.Name}";
        }
        return "unknown";
    }
}