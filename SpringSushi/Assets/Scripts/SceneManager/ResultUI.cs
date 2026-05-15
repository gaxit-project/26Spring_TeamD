using UnityEngine;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI totalScoreText;

    // ★ ボタン操作済みフラグ（Retry / ReturnToTitle どちらかが押されたら封鎖）
    private bool isButtonHandled = false;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            totalScoreText.text = $"{ScoreManager.Instance.TotalScore}円";
        }
        else
        {
            Debug.LogWarning("ScoreManagerが見つかりません。");
        }
    }

    public void Retry()
    {
        // ★ 連打ガード & 遷移中ガード
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        isButtonHandled = true;

        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        StageManager.Instance.RetryFromBeginning();
    }

    public void OnReturnToTitle()
    {
        // ★ 連打ガード & 遷移中ガード
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        isButtonHandled = true;

        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
    }
}