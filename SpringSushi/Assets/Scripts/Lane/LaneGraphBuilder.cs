using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneGraphBuilder : MonoBehaviour
{
    /// <summary>
    /// シーン内のSegmentとNodeを収集してグラフを構築する。
    /// NodeのconnectedSegmentsを自動設定し、(segments, nodes)を返す。
    /// </summary>
    public (List<LaneSegment> segments, List<LaneNode> nodes) Build()
    {
        var segments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();
        var nodes = FindObjectsByType<LaneNode>(FindObjectsSortMode.None).ToList();

        // connectedSegmentsをリセット（エディタ上の残存データ対策）
        foreach (var node in nodes)
            node.connectedSegments.Clear();

        foreach (var seg in segments)
        {
            // nodeA / nodeB はInspectorまたはLaneSegment側で保持している前提
            if (seg.nodeA != null && !seg.nodeA.connectedSegments.Contains(seg))
                seg.nodeA.connectedSegments.Add(seg);
            if (seg.nodeB != null && !seg.nodeB.connectedSegments.Contains(seg))
                seg.nodeB.connectedSegments.Add(seg);
        }

        Debug.Log($"<color=green>[GraphBuilder]</color> 構築完了: {segments.Count}セグメント / {nodes.Count}ノード");
        return (segments, nodes);
    }
}