using UnityEngine;
using TMPro; // TextMeshProを使うために追加

public class ResultUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI totalScoreText; // 合計スコアを表示するテキスト

    private void Start()
    {
        // シーン開始時にScoreManagerから合計スコアを取得して表示する
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
        // 次のゲームに向けてスコアをリセット
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        
        StageManager.Instance.RetryFromBeginning();
    }

    public void OnReturnToTitle()
    {
        // タイトルに戻る際もスコアをリセット
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();

        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
    }
}