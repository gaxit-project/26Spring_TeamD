using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BusinessHoursTimer : MonoBehaviour
{
    [SerializeField] private Slider businessTimeSlider;
    public bool IsInBusiness { get; private set; }

    /// <summary>
    /// 営業時間を開始します
    /// </summary>
    /// <param name="duration">営業時間（秒）</param>
    public IEnumerator StartBusiness(float duration)
    {
        IsInBusiness = true;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 進行度を 0.0 ～ 1.0 で更新
            businessTimeSlider.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        IsInBusiness = false;
    }
}