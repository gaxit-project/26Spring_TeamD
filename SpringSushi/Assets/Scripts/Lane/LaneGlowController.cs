using System.Collections.Generic;
using UnityEngine;

public enum GlowBlinkMode
{
    SmoothFade, // フワフワと呼吸するようにフェード点滅（おすすめ・UIと同調）
    HardBlink   // パッパッとチカチカ切り替わる点滅
}

public class LaneGlowController : MonoBehaviour
{
    [SerializeField] private LaneSelectorUI laneSelectorUI;

    [Header("点滅アニメーション設定")]
    [Tooltip("点滅のスタイルを選択")]
    [SerializeField] private GlowBlinkMode blinkMode = GlowBlinkMode.SmoothFade;

    [Tooltip("点滅の速さ（UIの fadeSpeed と同じ値にするとテンポが揃います）")]
    [SerializeField] private float blinkSpeed = 3.0f;

    [Range(0f, 1f)]
    [Tooltip("フェード時の最小アルファ（0で完全に消える、0.2?0.3にすると上品に残る）")]
    [SerializeField] private float minAlpha = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("フェード時の最大アルファ")]
    [SerializeField] private float maxAlpha = 1.0f;

    private List<LaneNode> allNodes = new();
    private List<LaneSegment> allSegments = new();

    private LaneColor? lastAppliedColor = null;
    private bool isInitialized = false;

    private void Awake()
    {
        if (laneSelectorUI == null)
            laneSelectorUI = FindObjectOfType<LaneSelectorUI>();

        RefreshTargets();
    }

    public void RefreshTargets()
    {
        allNodes = new List<LaneNode>(FindObjectsByType<LaneNode>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        allSegments = new List<LaneSegment>(FindObjectsByType<LaneSegment>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void Update()
    {
        if (laneSelectorUI == null) return;

        if (!laneSelectorUI.isActiveAndEnabled)
        {
            if (lastAppliedColor != null)
            {
                SetAllGlow(false);
                lastAppliedColor = null;
            }
            return;
        }

        if (allSegments.Count == 0 || allNodes.Count == 0)
        {
            RefreshTargets();
        }

        LaneColor? current = laneSelectorUI.CurrentSelectedColor;

        // 1. 色が切り替わった瞬間に、対象色だけをActiveにし、それ以外を完全に消灯
        if (!isInitialized || current != lastAppliedColor)
        {
            SwitchActiveColor(current);
            lastAppliedColor = current;
            isInitialized = true;
        }

        // 2. 選択中の色だけを毎フレーム点滅させる
        if (current.HasValue)
        {
            AnimateSelectedColorBlink(current.Value);
        }
    }

    private void SwitchActiveColor(LaneColor? activeColor)
    {
        foreach (var node in allNodes)
        {
            if (node == null) continue;
            node.SetGlow(activeColor.HasValue && node.laneColor == activeColor.Value);
        }

        foreach (var seg in allSegments)
        {
            if (seg == null) continue;
            seg.SetGlow(activeColor.HasValue && seg.laneColor == activeColor.Value);
        }
    }

    private void AnimateSelectedColorBlink(LaneColor activeColor)
    {
        float alpha = 1f;
        bool isHardVisible = true;

        if (blinkMode == GlowBlinkMode.SmoothFade)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        }
        else // HardBlink
        {
            isHardVisible = (Mathf.FloorToInt(Time.time * blinkSpeed) % 2) == 0;
            alpha = isHardVisible ? maxAlpha : 0f;
        }

        // 選択されている色だけを更新（非選択のレーンには触らないので超軽量）
        for (int i = 0; i < allSegments.Count; i++)
        {
            var seg = allSegments[i];
            if (seg != null && seg.laneColor == activeColor)
                seg.UpdateBlink(alpha, isHardVisible);
        }

        for (int i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            if (node != null && node.laneColor == activeColor)
                node.UpdateBlink(alpha, isHardVisible);
        }
    }

    private void SetAllGlow(bool visible)
    {
        foreach (var node in allNodes)
            if (node != null) node.SetGlow(visible);

        foreach (var seg in allSegments)
            if (seg != null) seg.SetGlow(visible);
    }
}