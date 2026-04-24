using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneGraphBuilder : MonoBehaviour
{
    public List<LaneSegment> Build()
    {
        var segments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();
        var nodes = FindObjectsByType<LaneNode>(FindObjectsSortMode.None).ToList();

        var map = new Dictionary<Vector3, LaneNode>();

        foreach (var node in nodes)
        {
            GetOrCreate(node, map);
        }

        foreach (var seg in segments)
        {
            var childNodes = seg.GetComponentsInChildren<LaneNode>();
            if (childNodes.Length < 2) continue;

            seg.nodeA = GetOrCreate(childNodes[0], map);
            seg.nodeB = GetOrCreate(childNodes[1], map);

            if (!seg.nodeA.connectedSegments.Contains(seg))
                seg.nodeA.connectedSegments.Add(seg);

            if (!seg.nodeB.connectedSegments.Contains(seg))
                seg.nodeB.connectedSegments.Add(seg);
        }

        Debug.Log($"<color=green>[GraphBuilder]</color> 構築完了: {segments.Count}セグメント / {map.Count}ノード");

        return segments;
    }

    private LaneNode GetOrCreate(LaneNode node, Dictionary<Vector3, LaneNode> map)
    {
        Vector3 key = Round(node.transform.position);

        if (map.ContainsKey(key))
            return map[key];

        map.Add(key, node);
        return node;
    }

    private Vector3 Round(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x * 100f) / 100f,
            Mathf.Round(v.y * 100f) / 100f,
            Mathf.Round(v.z * 100f) / 100f
        );
    }
}