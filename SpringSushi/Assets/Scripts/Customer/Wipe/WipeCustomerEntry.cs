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

        // 人のイラスト
        if (customerIcon != null)
            customerIcon.sprite = defaultCustomerSprite;

        // 吹き出しのアイコン
        if (moodIcon != null && data.mood != null)
            moodIcon.sprite = data.mood.moodIcon;
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