using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// HUD_Canvas上の固定位置に合計金額をアニメーション付きで表示する。
///
/// 【Hierarchy構成】
/// HUD_Canvas
///   └─ ScoreUI (ScoreUI.cs) ← このGameObjectにアタッチ
///        ├─ ScoreText (TextMeshProUGUI) ← 合計金額テキスト
///        └─ DeltaText (TextMeshProUGUI) ← +/-の変化量テキスト（PricePopupと同様にフェード）
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI scoreText;    // 合計金額を常時表示
    [SerializeField] private TextMeshProUGUI deltaText;    // +/-の変化量をアニメーション表示

    [Header("合計金額アニメーション")]
    [SerializeField] private float scoreCountDuration = 0.4f;  // 合計金額が変化するアニメーション時間
    [SerializeField] private Color positiveColor = Color.yellow;
    [SerializeField] private Color negativeColor = Color.red;

    [Header("変化量アニメーション（PricePopupと同様）")]
    [SerializeField] private float deltaFadeInDuration = 0.15f;
    [SerializeField] private float deltaHoldDuration = 0.5f;
    [SerializeField] private float deltaFadeOutDuration = 0.4f;
    [SerializeField] private float deltaRiseDistance = 40f;

    private int displayedScore = 0;
    private Coroutine scoreCountCoroutine;
    private Coroutine deltaAnimCoroutine;

    private void Start()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;

        UpdateScoreText(0);
        if (deltaText != null) deltaText.color = Color.clear;
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int delta, int total)
    {
        // 合計金額アニメーション
        if (scoreCountCoroutine != null) StopCoroutine(scoreCountCoroutine);
        scoreCountCoroutine = StartCoroutine(CountScore(displayedScore, total));

        // 変化量アニメーション（delta=0はリセット）
        if (delta != 0)
        {
            if (deltaAnimCoroutine != null) StopCoroutine(deltaAnimCoroutine);
            deltaAnimCoroutine = StartCoroutine(AnimateDelta(delta));
        }
    }

    /// <summary>
    /// 合計金額をカウントアップ/ダウンしながら表示する。
    /// </summary>
    private IEnumerator CountScore(int from, int to)
    {
        float elapsed = 0f;
        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scoreCountDuration);
            // イーズアウト
            t = 1f - Mathf.Pow(1f - t, 3f);
            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            displayedScore = current;
            UpdateScoreText(current);
            yield return null;
        }
        displayedScore = to;
        UpdateScoreText(to);
    }

    /// <summary>
    /// 変化量をフェードイン→ホールド→上へフェードアウトで表示する。
    /// </summary>
    private IEnumerator AnimateDelta(int delta)
    {
        if (deltaText == null) yield break;

        bool isPositive = delta > 0;
        string prefix = isPositive ? "+" : "";
        Color baseColor = isPositive ? positiveColor : negativeColor;

        deltaText.text = $"{prefix}{delta}";
        Vector3 startPos = deltaText.rectTransform.localPosition;

        // フェードイン
        float t = 0f;
        while (t < deltaFadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / deltaFadeInDuration);
            deltaText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        // ホールド
        deltaText.color = baseColor;
        yield return new WaitForSeconds(deltaHoldDuration);

        // フェードアウト（上へ移動）
        t = 0f;
        while (t < deltaFadeOutDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / deltaFadeOutDuration);
            deltaText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - ratio);
            deltaText.rectTransform.localPosition = startPos + Vector3.up * (deltaRiseDistance * ratio);
            yield return null;
        }

        deltaText.color = Color.clear;
        deltaText.rectTransform.localPosition = startPos;
    }

    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:N0}円";
    }
}