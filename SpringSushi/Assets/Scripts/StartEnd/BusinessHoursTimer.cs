using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BusinessHoursTimer : MonoBehaviour
{
    [SerializeField] private Image businessTimeImage;
    public bool IsInBusiness { get; private set; }

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

    /// <summary>
    /// ★ 追加：客が全員帰った等の理由で営業時間を強制終了する。
    /// 外部からStartBusinessのコルーチンをStopCoroutineした直後に呼ぶこと。
    /// </summary>
    public void ForceComplete()
    {
        if (!IsInBusiness) return;
        if (businessTimeImage != null)
            businessTimeImage.fillAmount = 1f;
        IsInBusiness = false;
    }
}