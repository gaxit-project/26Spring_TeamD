using System.Collections.Generic;
using UnityEngine;

public enum GlowBlinkMode
{
    SmoothFade,
    HardBlink
}

public class LaneGlowController : MonoBehaviour
{
    [SerializeField] private LaneSelectorUI laneSelectorUI;
    [SerializeField] private LaneColorButtonTracker colorButtonTracker;

    [Header("点滅アニメーション設定")]
    [Tooltip("点滅のスタイルを選択")]
    [SerializeField] private GlowBlinkMode blinkMode = GlowBlinkMode.SmoothFade;

    [Tooltip("点滅の速さ")]
    [SerializeField] private float blinkSpeed = 3.0f;

    [Range(0f, 1f)]
    [Tooltip("フェード時の最小アルファ")]
    [SerializeField] private float minAlpha = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("フェード時の最大アルファ")]
    [SerializeField] private float maxAlpha = 1.0f;

    private List<LaneNode> allNodes = new();
    private List<LaneSegment> allSegments = new();

    private LaneColor? lastAppliedColor = null;
    private bool isInitialized = false;

    // 赤Flameフラッシュ演出用
    private LaneColor? redFlashingColor = null;
    private float redFlashTimer = 0f;

    private void Awake()
    {
        if (laneSelectorUI == null)
            laneSelectorUI = FindObjectOfType<LaneSelectorUI>();

        if (colorButtonTracker == null)
            colorButtonTracker = FindObjectOfType<LaneColorButtonTracker>();

        RefreshTargets();
    }

    private void OnEnable()
    {
        if (colorButtonTracker != null)
            colorButtonTracker.OnPressFlashed += HandlePressFlashed;
    }

    private void OnDisable()
    {
        if (colorButtonTracker != null)
            colorButtonTracker.OnPressFlashed -= HandlePressFlashed;

        SetAllRedGlow(false);
    }

    private void HandlePressFlashed(LaneColor color, float duration)
    {
        redFlashingColor = color;
        redFlashTimer = duration;

        // 赤Flameを表示し、通常Flameは一時的に消灯する
        SetLaneRedGlow(color, true);
        SetLaneNormalGlow(color, false);
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
            if (lastAppliedColor != null || redFlashingColor != null)
            {
                SetAllGlow(false);
                SetAllRedGlow(false);
                lastAppliedColor = null;
                redFlashingColor = null;
            }
            return;
        }

        if (allSegments.Count == 0 || allNodes.Count == 0)
        {
            RefreshTargets();
        }

        // 赤フラッシュ演出のタイマー更新
        if (redFlashTimer > 0f)
        {
            redFlashTimer -= Time.deltaTime;
            if (redFlashTimer <= 0f)
            {
                if (redFlashingColor.HasValue)
                {
                    SetLaneRedGlow(redFlashingColor.Value, false);
                    redFlashingColor = null;
                }
                // 通常のハイライト表示を復帰
                lastAppliedColor = null; // 次のステップで再適用させる
            }
        }

        LaneColor? current = laneSelectorUI.CurrentSelectedColor;

        // 1. 色が切り替わった瞬間に、対象色だけをActiveにし、それ以外を消灯
        if (!isInitialized || current != lastAppliedColor)
        {
            SwitchActiveColor(current);
            lastAppliedColor = current;
            isInitialized = true;
        }

        // 2. 選択中の色だけを毎フレーム点滅（赤フラッシュ中はその色を点滅させない）
        if (current.HasValue && current.Value != redFlashingColor)
        {
            AnimateSelectedColorBlink(current.Value);
        }
    }

    private void SwitchActiveColor(LaneColor? activeColor)
    {
        foreach (var node in allNodes)
        {
            if (node == null) continue;
            bool isCurrent = activeColor.HasValue && node.laneColor == activeColor.Value;
            // 赤フラッシュ中の色は通常のFlameをつけない
            node.SetGlow(isCurrent && node.laneColor != redFlashingColor);
        }

        foreach (var seg in allSegments)
        {
            if (seg == null) continue;
            bool isCurrent = activeColor.HasValue && seg.laneColor == activeColor.Value;
            seg.SetGlow(isCurrent && seg.laneColor != redFlashingColor);
        }
    }

    private void SetLaneNormalGlow(LaneColor color, bool visible)
    {
        foreach (var node in allNodes)
            if (node != null && node.laneColor == color) node.SetGlow(visible);

        foreach (var seg in allSegments)
            if (seg != null && seg.laneColor == color) seg.SetGlow(visible);
    }

    private void SetLaneRedGlow(LaneColor color, bool visible)
    {
        foreach (var node in allNodes)
            if (node != null && node.laneColor == color) node.SetRedGlow(visible);

        foreach (var seg in allSegments)
            if (seg != null && seg.laneColor == color) seg.SetRedGlow(visible);
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
        else
        {
            isHardVisible = (Mathf.FloorToInt(Time.time * blinkSpeed) % 2) == 0;
            alpha = isHardVisible ? maxAlpha : 0f;
        }

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

    private void SetAllRedGlow(bool visible)
    {
        foreach (var node in allNodes)
            if (node != null) node.SetRedGlow(visible);

        foreach (var seg in allSegments)
            if (seg != null) seg.SetRedGlow(visible);
    }
}