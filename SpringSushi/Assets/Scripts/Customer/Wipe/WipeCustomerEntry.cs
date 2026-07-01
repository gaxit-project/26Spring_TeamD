using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ワイプ上の客1人分のUI。
/// イラスト＋吹き出しを表示する。
/// </summary>
public class WipeCustomerEntry : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Image customerIcon;
    [SerializeField] private Image speechBubble;
    [SerializeField] private Image moodIcon;

    private WaitingCustomerData waitingData;
    public WaitingCustomerData WaitingData => waitingData;

    public void Initialize(WaitingCustomerData data, Sprite defaultCustomerSprite)
    {
        waitingData = data;
        if (customerIcon != null)
            customerIcon.sprite = defaultCustomerSprite;

        bool isRandom = data.mood == null || data.mood.moodType == CustomerMoodSO.MoodType.Random;
        if (moodIcon != null)
        {
            moodIcon.sprite = isRandom ? null : data.mood.moodIcon;
            var c = moodIcon.color;
            c.a = isRandom ? 0f : 1f;
            moodIcon.color = c;
        }
        if (speechBubble != null)
        {
            var c = speechBubble.color;
            c.a = isRandom ? 0f : 1f;
            speechBubble.color = c;
        }
    }

    /// <summary>
    /// ★ 追加：Wipe上をドア位置まで歩く。
    /// 到着後にonArrivedを呼び、自身を削除する。
    /// </summary>
    public void WalkToDoor(float doorX, float duration, System.Action onArrived)
    {
        StartCoroutine(WalkToDoorCoroutine(doorX, duration, onArrived));
    }

    private IEnumerator WalkToDoorCoroutine(float doorX, float duration, System.Action onArrived)
    {
        var rect = GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;
        Vector2 end = new Vector2(doorX, start.y);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rect.anchoredPosition = end;

        // ★ ドア到着 → ゲーム内スポーンを通知してから消える
        onArrived?.Invoke();
        Destroy(gameObject);
    }

    // ───────────────────────────────────────────────
    // ★ 旧・即時入店アニメーション。WalkToDoorに置き換えたためコメントアウト
    // ───────────────────────────────────────────────
    /*
    public void PlayEnterAnimation(System.Action onComplete)
    {
        StartCoroutine(EnterCoroutine(onComplete));
    }
    private System.Collections.IEnumerator EnterCoroutine(System.Action onComplete)
    {
        var rect = GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;
        Vector2 end = start + new Vector2(-80f, 0f);
        float t = 0f;
        float dur = 0.3f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(start, end, t / dur);
            yield return null;
        }
        onComplete?.Invoke();
        Destroy(gameObject);
    }
    */
}