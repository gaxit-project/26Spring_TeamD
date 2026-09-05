using System.Collections.Generic;
using UnityEngine;

public class LaneGlowController : MonoBehaviour
{
    [SerializeField] private LaneSelectorUI laneSelectorUI;

    private List<LaneNode> allNodes = new();
    private List<LaneSegment> allSegments = new();

    private LaneColor? lastAppliedColor = null;
    private bool isInitialized = false;

    private void Awake()
    {
        if (laneSelectorUI == null)
        {
            laneSelectorUI = FindObjectOfType<LaneSelectorUI>();
        }

        RefreshTargets();
    }

    /// <summary>
    /// シーン上の全ノード・セグメントを再収集する（動的生成対応）
    /// </summary>
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

        // ステージが後から生成された場合、件数が0件なら自動で再取得を試みる
        if (allSegments.Count == 0 || allNodes.Count == 0)
        {
            RefreshTargets();
        }

        LaneColor? current = laneSelectorUI.CurrentSelectedColor;

        if (!isInitialized || current != lastAppliedColor)
        {
            ApplyGlow(current);
            lastAppliedColor = current;
            isInitialized = true;
        }
    }

    private void ApplyGlow(LaneColor? color)
    {
        // 取得できている件数をログに出す
        Debug.Log($"<color=yellow>[LaneGlowController]</color> 発光を更新: {(color.HasValue ? color.Value.ToString() : "なし")} (対象: Segment {allSegments.Count}件, Node {allNodes.Count}件)");

        foreach (var node in allNodes)
        {
            if (node == null) continue;
            node.SetGlow(color.HasValue && node.laneColor == color.Value);
        }

        foreach (var seg in allSegments)
        {
            if (seg == null) continue;
            seg.SetGlow(color.HasValue && seg.laneColor == color.Value);
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