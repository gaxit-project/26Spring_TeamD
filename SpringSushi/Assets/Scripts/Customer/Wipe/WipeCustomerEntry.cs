using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    /// 指定X座標まで移動する。
    /// ★ destroyOnComplete=trueのときのみ到着後にDestroy。
    ///    2段階アニメーションの1段目ではfalseを渡すこと。
    /// </summary>
    public void WalkToDoor(float targetX, float duration, System.Action onArrived,
                           bool destroyOnComplete = true)
    {
        StartCoroutine(WalkToDoorCoroutine(targetX, duration, onArrived, destroyOnComplete));
    }

    private IEnumerator WalkToDoorCoroutine(float targetX, float duration,
                                             System.Action onArrived, bool destroyOnComplete)
    {
        var rect = GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        rect.anchoredPosition = end;
        onArrived?.Invoke();

        // ★ 修正：destroyOnCompleteがtrueのときだけDestroyする
        if (destroyOnComplete)
            Destroy(gameObject);
    }

    /// <summary>
    /// 指定座標へアニメーション移動する汎用メソッド。
    /// EnqueueCustomer時のキュー位置移動に使用。
    /// </summary>
    public void MoveToPosition(Vector2 targetPos, float duration)
    {
        StartCoroutine(MoveCoroutine(targetPos, duration));
    }

    private IEnumerator MoveCoroutine(Vector2 targetPos, float duration)
    {
        var rect = GetComponent<RectTransform>();
        Vector2 start = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }
}