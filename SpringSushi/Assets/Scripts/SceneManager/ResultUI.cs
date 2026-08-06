using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// ResultScene のUI。売上金額と星評価を表示する。
/// </summary>
public class ResultUI : MonoBehaviour
{
    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI totalScoreText;

    [Header("スコアカウントアップ演出")]
    [SerializeField] private float scoreCountDuration = 1.2f; // 0→合計金額までのカウントアップ時間
    [SerializeField] private int soundStepAmount = 1000;      // この金額増えるごとにチャリン音を1回鳴らす

    [Header("星評価UI（3つ並べる）")]
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;
    [SerializeField] private Sprite starFilled;   // ★ 点灯
    [SerializeField] private Sprite starEmpty;    // ☆ 消灯

    [Header("星演出設定")]
    [SerializeField] private float starRevealInterval = 0.5f;  // 星が1つずつ出る間隔
    [SerializeField] private float starScalePunch = 1.4f;      // ポップのスケール倍率
    [SerializeField] private float starPunchDuration = 0.25f;  // ポップの時間

    private bool isButtonHandled = false;

    private void Start()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;
        int stars = StageManager.Instance?.CurrentGoal?.GetStarCount(score) ?? 0;

        // 星を一旦すべて消灯
        SetStarSprite(star1, false);
        SetStarSprite(star2, false);
        SetStarSprite(star3, false);

        UpdateScoreText(0);

        StartCoroutine(PlayResultSequence(score, stars));
    }

    /// <summary>
    /// 「0から金額がカウントアップ→星が1つずつ点灯」の一連の演出を順番に行う。
    /// </summary>
    private IEnumerator PlayResultSequence(int score, int stars)
    {
        yield return CountUpScore(score);
        yield return RevealStars(stars);
    }

    /// <summary>
    /// 0からscoreまで金額をカウントアップする。
    /// soundStepAmount円増えるごとにチャリーン音(ScoreUp)を鳴らし、複数回チャリンチャリンと聞こえるようにする。
    /// </summary>
    private IEnumerator CountUpScore(int score)
    {
        if (score <= 0)
        {
            UpdateScoreText(score);
            yield break;
        }

        int lastPlayedAt = 0;
        PlayScoreUpSound(); // 開始時点で1回鳴らす

        float elapsed = 0f;
        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scoreCountDuration);
            t = 1f - Mathf.Pow(1f - t, 3f); // イーズアウト
            int current = Mathf.RoundToInt(Mathf.Lerp(0, score, t));
            UpdateScoreText(current);

            // 表示金額がsoundStepAmount円以上進むたびに1回鳴らす
            if (soundStepAmount > 0 && current - lastPlayedAt >= soundStepAmount)
            {
                lastPlayedAt = current;
                PlayScoreUpSound();
            }

            yield return null;
        }

        UpdateScoreText(score);
    }

    private void PlayScoreUpSound()
    {
        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.PlaySFX(SoundKeys.ScoreUp);
    }

    /// <summary>星を1つずつアニメーション付きで点灯させる</summary>
    private IEnumerator RevealStars(int count)
    {
        Image[] stars = { star1, star2, star3 };
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(starRevealInterval);
            bool lit = i < count;
            SetStarSprite(stars[i], lit);
            if (lit && stars[i] != null)
                StartCoroutine(PunchScale(stars[i].transform));
        }
    }

    /// <summary>スケールをポンとはじくアニメーション</summary>
    private IEnumerator PunchScale(Transform t)
    {
        Vector3 original = t.localScale;
        float elapsed = 0f;
        while (elapsed < starPunchDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / starPunchDuration;
            // 0→1→0 の山なりカーブ
            float scale = 1f + (starScalePunch - 1f) * Mathf.Sin(ratio * Mathf.PI);
            t.localScale = original * scale;
            yield return null;
        }
        t.localScale = original;
    }

    private void SetStarSprite(Image img, bool filled)
    {
        if (img == null) return;
        img.sprite = filled ? starFilled : starEmpty;
    }

    private void UpdateScoreText(int score)
    {
        if (totalScoreText != null)
            totalScoreText.text = $"{score:N0}円";
    }

    // -------------------------------------------------------
    // ボタン
    // -------------------------------------------------------
    public void Retry()
    {
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        isButtonHandled = true;
        LockSoundOnEventSystem();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        GameStateManager.Instance.EnterReady();
        StageManager.Instance.RetryFromBeginning();
    }

    public void OnReturnToTitle()
    {
        if (isButtonHandled) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        isButtonHandled = true;
        LockSoundOnEventSystem();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
    }

    private void LockSoundOnEventSystem()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        selected?.GetComponent<UIButtonSound>()?.Lock();
    }
}