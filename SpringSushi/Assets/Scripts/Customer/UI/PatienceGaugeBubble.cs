using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PatienceGaugeBubble : MonoBehaviour
{
    [Header("液面 Image")]
    public Image fillImage;

    [Header("Eating中に表示するイラスト")]
    public GameObject eatingOverlay; // Eating中に表示するImageオブジェクト

    [Header("注文アイコン親")]
    public RectTransform orderIconRoot;

    private static readonly Color ColorFull = new Color(0.27f, 0.93f, 0.27f);
    private static readonly Color ColorMid = new Color(1.00f, 0.92f, 0.23f);
    private static readonly Color ColorWarning = new Color(1.00f, 0.55f, 0.10f);
    private static readonly Color ColorDanger = new Color(0.95f, 0.25f, 0.25f);

    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        if (fillImage != null)
        {
            fillImage.fillAmount = value;
            fillImage.color = EvaluateColor(value);
        }
        if (orderIconRoot != null)
            orderIconRoot.SetAsLastSibling();
    }

    /// <summary>
    /// OrderPhaseに応じて表示を切り替える。
    /// </summary>
    public void SetOrderPhase(CustomerAI.OrderPhase phase)
    {
        bool isEating = phase == CustomerAI.OrderPhase.PartiallyServed
                     || phase == CustomerAI.OrderPhase.BatchComplete;

        if (eatingOverlay != null)
            eatingOverlay.SetActive(isEating);

        // Eating中はゲージを非表示
        if (fillImage != null)
            fillImage.gameObject.SetActive(!isEating);
    }

    /// <summary>
    /// バッチ開始時にゲージをリセットして通常表示に戻す。
    /// </summary>
    public void ResetToFull()
    {
        SetOrderPhase(CustomerAI.OrderPhase.Waiting);
        SetValue(1f);
    }

    private static Color EvaluateColor(float value)
    {
        if (value > 0.67f) return Color.Lerp(ColorMid, ColorFull, (value - 0.67f) / 0.33f);
        if (value > 0.33f) return Color.Lerp(ColorWarning, ColorMid, (value - 0.33f) / 0.34f);
        return Color.Lerp(ColorDanger, ColorWarning, value / 0.33f);
    }
}