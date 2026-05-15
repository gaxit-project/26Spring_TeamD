using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    // ★ Pause/Resume の処理中に多重呼び出しされるのを防ぐクールダウンフラグ
    private bool isHandling = false;

    public void Pause()
    {
        if (isHandling) return;

        var state = GameStateManager.Instance?.CurrentState;
        Debug.Log($"[PauseMenuUI] Pause() 呼び出し (GameState:{state})");

        if (GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.IsReady)
        {
            Debug.LogWarning("[PauseMenuUI] ★ Ready中のためPause拒否");
            return;
        }
        // すでにPause済みなら二重適用しない
        if (GameStateManager.Instance.IsPaused)
        {
            Debug.LogWarning("[PauseMenuUI] ★ すでにPause中のためスキップ");
            return;
        }

        isHandling = true;

        pauseUI.SetActive(true);
        GameStateManager.Instance.PauseGame();

        isHandling = false;
    }

    public void Resume()
    {
        if (isHandling) return;

        Debug.Log("[PauseMenuUI] Resume() 呼び出し");

        if (GameStateManager.Instance == null) return;
        // Pause中でなければ Resume しない
        if (!GameStateManager.Instance.IsPaused)
        {
            Debug.LogWarning("[PauseMenuUI] ★ Pause中ではないためResumeをスキップ");
            return;
        }

        isHandling = true;

        pauseUI.SetActive(false);
        GameStateManager.Instance.ResumeGame();

        isHandling = false;
    }

    public void ReturnToTitle()
    {
        if (isHandling) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        Debug.Log("[PauseMenuUI] ReturnToTitle() 呼び出し");

        isHandling = true;

        GameStateManager.Instance.ResumeGame();
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");

        // ★ シーン遷移に渡るため、このオブジェクトは破棄される想定。
        //    念のため UI を非表示にしてこれ以上の操作を受け付けない。
        pauseUI.SetActive(false);
    }
}