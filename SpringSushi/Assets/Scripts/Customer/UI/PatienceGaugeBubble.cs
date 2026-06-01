using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 吹き出し型の忍耐ゲージ。
/// 液面が上から下に向かって下がり、色が緑→黄→オレンジ→赤に変化する。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PatienceGaugeBubble : MonoBehaviour
{
    [Header("液面 Image（Fill方式）")]
    [Tooltip("吹き出しマスクの内側に置いた Image。ImageType=Filled, FillMethod=Vertical, FillOrigin=Top を設定してください")]
    public Image fillImage;

    [Header("注文アイコン親（最前面に置く）")]
    [Tooltip("orderImageContainer の RectTransform。fillImage より高い SiblingIndex に設定してください")]
    public RectTransform orderIconRoot;

    // 緑 → 黄 → オレンジ → 赤
    private static readonly Color ColorFull = new Color(0.27f, 0.93f, 0.27f); // 緑
    private static readonly Color ColorMid = new Color(1.00f, 0.92f, 0.23f); // 黄
    private static readonly Color ColorWarning = new Color(1.00f, 0.55f, 0.10f); // オレンジ
    private static readonly Color ColorDanger = new Color(0.95f, 0.25f, 0.25f); // 赤

    /// <summary>
    /// value : 1（満タン）→ 0（空）
    /// </summary>
    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);

        // --- 液面の高さを更新 ---
        if (fillImage != null)
        {
            fillImage.fillAmount = value;
            fillImage.color = EvaluateColor(value);
        }

        // --- アイコンを最前面に（念のため毎フレームでなくSetValue時に保証） ---
        if (orderIconRoot != null)
            orderIconRoot.SetAsLastSibling();
    }

    /// <summary>
    /// value 1→0 に対して 緑→黄→オレンジ→赤 をグラデーション
    /// </summary>
    private static Color EvaluateColor(float value)
    {
        // value: 1.0=緑  0.67=黄  0.33=オレンジ  0.0=赤
        if (value > 0.67f)
            return Color.Lerp(ColorMid, ColorFull, (value - 0.67f) / 0.33f);
        if (value > 0.33f)
            return Color.Lerp(ColorWarning, ColorMid, (value - 0.33f) / 0.34f);
        return Color.Lerp(ColorDanger, ColorWarning, value / 0.33f);
    }
}