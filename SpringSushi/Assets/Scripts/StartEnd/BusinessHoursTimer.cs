using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BusinessHoursTimer : MonoBehaviour
{
    [SerializeField] private Slider businessTimeSlider;

    public bool IsInBusiness { get; private set; }

    /// <summary>
    /// 営業時間を開始します。多重呼び出しは無視されます。
    /// </summary>
    /// <param name="duration">営業時間（秒）</param>
    public IEnumerator StartBusiness(float duration)
    {
        // 多重起動ガード: すでに営業中なら即終了
        if (IsInBusiness)
        {
            Debug.LogWarning("[BusinessTimer] StartBusiness called while already in business. Ignored.");
            yield break;
        }

        IsInBusiness = true;

        // スライダーを初期化
        if (businessTimeSlider != null)
            businessTimeSlider.value = 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (businessTimeSlider != null)
                businessTimeSlider.value = Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        // 終端を確定
        if (businessTimeSlider != null)
            businessTimeSlider.value = 1f;

        IsInBusiness = false;
    }
}