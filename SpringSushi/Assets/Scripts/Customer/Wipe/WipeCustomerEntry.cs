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

    // ★追加：現在動いているコルーチンを1つだけ保持する（常にこれ経由で動かす）
    private Coroutine currentMoveCoroutine;

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
    /// 指定X座標まで移動する（入店演出用）。
    /// ★ 既に動いているコルーチンがあれば必ず止めてから開始する。
    /// </summary>
    public void WalkToDoor(float targetX, float duration, System.Action onArrived,
                           bool destroyOnComplete = true)
    {
        StartManagedMove(WalkToDoorCoroutine(targetX, duration, onArrived, destroyOnComplete));
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
            t = t * t * (3f - 2f * t); // ★追加：スムーズステップで自然な加減速に
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        rect.anchoredPosition = end;
        currentMoveCoroutine = null;
        onArrived?.Invoke();
        if (destroyOnComplete)
            Destroy(gameObject);
    }

    /// <summary>
    /// 指定座標へアニメーション移動する汎用メソッド（キュー整列用）。
    /// ★ 既に動いているコルーチンがあれば必ず止めてから開始する。
    /// </summary>
    public void MoveToPosition(Vector2 targetPos, float duration)
    {
        StartManagedMove(MoveCoroutine(targetPos, duration));
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
            t = t * t * (3f - 2f * t); // ★追加：スムーズステップ
            rect.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }
        rect.anchoredPosition = targetPos;
        currentMoveCoroutine = null;
    }

    // ★追加：どちらの移動でも必ずここを経由させることで、
    //   「前の移動が終わっていないのに新しい移動が始まる」二重実行を防ぐ
    private void StartManagedMove(IEnumerator routine)
    {
        if (currentMoveCoroutine != null)
            StopCoroutine(currentMoveCoroutine);
        currentMoveCoroutine = StartCoroutine(routine);
    }
}