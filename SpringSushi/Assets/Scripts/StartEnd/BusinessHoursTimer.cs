using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BusinessHoursTimer : MonoBehaviour
{
    [SerializeField] private Image businessTimeImage; // ImageType=Filled の円形Image
    public bool IsInBusiness { get; private set; }

    /// <summary>
    /// 営業時間を開始します。多重呼び出しは無視されます。
    /// </summary>
    /// <param name="duration">営業時間（秒）</param>
    public IEnumerator StartBusiness(float duration)
    {
        if (IsInBusiness)
        {
            Debug.LogWarning("[BusinessTimer] StartBusiness called while already in business. Ignored.");
            yield break;
        }

        IsInBusiness = true;

        if (businessTimeImage != null)
            businessTimeImage.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (businessTimeImage != null)
                businessTimeImage.fillAmount = Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        if (businessTimeImage != null)
            businessTimeImage.fillAmount = 1f;

        IsInBusiness = false;
    }
}