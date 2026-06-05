using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StartEnd_Canvas にアタッチ。
/// ステージロード後に売上目標を表示し、表示完了後にスタートボタンを出す。
///
/// 【Hierarchy への追加要素】
/// StartEnd_Canvas
///   └─ GoalPanel (このスクリプトをアタッチ)
///        ├─ GoalTitleText   (TextMeshProUGUI) "売上目標"
///        ├─ StageNameText   (TextMeshProUGUI) ステージ名
///        ├─ BronzeRow
///        │    ├─ StarImage1 (Image) ★
///        │    └─ GoalText1  (TextMeshProUGUI) "1,500円"
///        ├─ SilverRow
///        │    ├─ StarImage2 (Image) ★★
///        │    └─ GoalText2  (TextMeshProUGUI) "3,000円"
///        ├─ GoldRow
///        │    ├─ StarImage3 (Image) ★★★
///        │    └─ GoalText3  (TextMeshProUGUI) "10,000円"
///        └─ StartPrompt     (GameObject)  "ボタンを押してスタート"
/// </summary>
public class StageGoalUI : MonoBehaviour
{
    [Header("テキスト参照")]
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI goalBronzeText;
    [SerializeField] private TextMeshProUGUI goalSilverText;
    [SerializeField] private TextMeshProUGUI goalGoldText;

    [Header("スタート促進UI（表示完了後に出す）")]
    [SerializeField] private GameObject startPrompt;

    [Header("表示演出設定")]
    [Tooltip("GoalPanelが表示されてから各行が順番に出るまでの間隔（秒）")]
    [SerializeField] private float rowInterval = 0.4f;

    [Tooltip("全行表示後、スタートボタンが出るまでの待機時間（秒）")]
    [SerializeField] private float waitBeforePrompt = 2.9f;

    [Tooltip("全行表示後、入力を受け付けるまでの最低待機時間（秒）\n waitBeforePrompt 以上に設定してください")]
    [SerializeField] private float inputUnlockDelay = 3.0f;

    [Header("CanvasGroup（フェード演出用）")]
    [SerializeField] private CanvasGroup bronzeRow;
    [SerializeField] private CanvasGroup silverRow;
    [SerializeField] private CanvasGroup goldRow;

    // InGameSequenceManager が参照する「入力解放済みか」フラグ
    public bool IsReadyToStart { get; private set; } = false;

    private void Start()
    {
        // 各行を最初は非表示
        SetRowAlpha(bronzeRow, 0f);
        SetRowAlpha(silverRow, 0f);
        SetRowAlpha(goldRow, 0f);
        if (startPrompt != null) startPrompt.SetActive(false);

        StartCoroutine(ShowGoalSequence());
    }

    private IEnumerator ShowGoalSequence()
    {
        var goal = StageManager.Instance?.CurrentGoal;
        if (goal == null)
        {
            // 目標データがなければ即解放
            Debug.LogWarning("[StageGoalUI] StageGoalData が null のため目標表示をスキップします。");
            IsReadyToStart = true;
            if (startPrompt != null) startPrompt.SetActive(true);
            yield break;
        }

        // ステージ名
        if (stageNameText != null)
            stageNameText.text = goal.stageName;

        // 目標金額テキスト設定
        if (goalBronzeText != null) goalBronzeText.text = $"{goal.goalBronze:N0}円";
        if (goalSilverText != null) goalSilverText.text = $"{goal.goalSilver:N0}円";
        if (goalGoldText != null) goalGoldText.text = $"{goal.goalGold:N0}円";

        // 各行をフェードインしながら順番に表示
        yield return FadeInRow(bronzeRow);
        yield return new WaitForSecondsRealtime(rowInterval);
        yield return FadeInRow(silverRow);
        yield return new WaitForSecondsRealtime(rowInterval);
        yield return FadeInRow(goldRow);

        // 全行表示後、waitBeforePrompt 秒待ってスタートボタンを出す
        yield return new WaitForSecondsRealtime(waitBeforePrompt);
        if (startPrompt != null) startPrompt.SetActive(true);

        // inputUnlockDelay まで残りを待って入力解放
        float remaining = inputUnlockDelay - waitBeforePrompt;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        IsReadyToStart = true;
        Debug.Log("[StageGoalUI] 入力解放 → スタート待ち");
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FadeInRow(CanvasGroup cg, float duration = 0.3f)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private void SetRowAlpha(CanvasGroup cg, float alpha)
    {
        if (cg != null) cg.alpha = alpha;
    }
}