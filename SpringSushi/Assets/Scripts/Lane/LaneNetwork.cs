using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    public List<LaneSegment> allSegments = new List<LaneSegment>();
    public List<LaneNode> allNodes = new List<LaneNode>();
    [SerializeField] private float snapThreshold = 0.5f; // 結合を許容する距離

    private void Awake()
    {
        InitializeGraph();
    }

    private void OnEnable()
    {
        // 入力イベントに自分の関数を登録（紐づけ）
        LaneInputManager.OnLaneButtonPressed += ToggleLanes;
    }

    private void OnDisable()
    {
        // メモリリーク防止のため解除
        LaneInputManager.OnLaneButtonPressed -= ToggleLanes;
    }

    /// <summary>
    /// シーン上の辺と点をつなげる関数
    /// </summary>
    private void InitializeGraph()
    {
        allSegments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();
        allNodes = FindObjectsByType<LaneNode>(FindObjectsSortMode.None).ToList();

        foreach (var seg in allSegments)
        {
            // 配列が正しく設定されているかチェック
            if (seg.connectionPoints == null || seg.connectionPoints.Length < 2)
            {
                Debug.LogWarning($"{seg.name} の接続ポイントが足りません！");
                continue;
            }

            // 1番目のポイントから近いNodeを探す
            seg.nodeA = FindClosestNode(seg.connectionPoints[0].position);
            // 2番目のポイントから近いNodeを探す
            seg.nodeB = FindClosestNode(seg.connectionPoints[1].position);

            if (seg.nodeA != null && !seg.nodeA.connectedSegments.Contains(seg))
                seg.nodeA.connectedSegments.Add(seg);
            if (seg.nodeB != null && !seg.nodeB.connectedSegments.Contains(seg))
                seg.nodeB.connectedSegments.Add(seg);
        }
    }

    // ステージ上のレーンを全て走査して反転する対象を検索する関数
    public void ToggleLanes(LaneColor color)
    {
        foreach (var seg in allSegments)
        {
            if (seg.laneColor == color)
            {
                seg.Reverse();
            }
        }
    }

    private LaneNode FindClosestNode(Vector3 pos)
    {
        LaneNode closest = null;
        float minDistance = snapThreshold;

        foreach (var node in allNodes)
        {
            float dist = Vector3.Distance(pos, node.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }
        return closest;
    }
}
