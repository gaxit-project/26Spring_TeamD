using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ワイプ上の客1人分のUI。
/// イラスト＋吹き出しを表示する。
/// </summary>
public class WipeCustomerEntry : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Image customerIcon;     // 人のイラスト
    [SerializeField] private Image speechBubble;     // 吹き出し背景
    [SerializeField] private Image moodIcon;         // 吹き出し内のアイコン

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
    /// 入店アニメーション（左へ移動して消える）
    /// </summary>
    public void PlayEnterAnimation(System.Action onComplete)
    {
        StartCoroutine(EnterCoroutine(onComplete));
    }

    private System.Collections.IEnumerator EnterCoroutine(System.Action onComplete)
    {
        var rect = GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;
        Vector2 end = start + new Vector2(-80f, 0f); // 左へ移動
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
}