using UnityEngine;

public class TitleUI : MonoBehaviour
{
    // ★ ボタン操作済みフラグ（いずれかのボタンが確定したら以降を封鎖）
    private bool isButtonHandled = false;

    public void OnStartButton()
    {
        // ★ 連打ガード & 遷移中ガード
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        Debug.Log($"[TitleUI] OnStartButton() 押下 (GameState:{GameStateManager.Instance?.CurrentState})");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[TitleUI] ★ GameStateManager.Instanceがnullです！");
            return;
        }

        isButtonHandled = true;

        GameStateManager.Instance.EnterReady();
        StageManager.Instance.StartFirstStage();
    }

    public void OnStageSelectButton()
    {
        // ★ 連打ガード & 遷移中ガード
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        Debug.Log("[TitleUI] OnStageSelectButton()");

        isButtonHandled = true;

        SceneController.Instance.LoadSceneAsync("StageSelectScene");
    }

    public void OnExitButton()
    {
        // 終了処理は連打しても副作用がないため軽量ガードのみ
        if (isButtonHandled) return;
        isButtonHandled = true;

        Application.Quit();
    }
}