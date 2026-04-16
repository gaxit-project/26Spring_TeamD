using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    public List<LaneSegment> allSegments = new List<LaneSegment>();
    public List<LaneNode>allNodes = new List<LaneNode>();

    private void Awake()
    {
        InitializeGraph();
    }

    /// <summary>
    /// シーン上のSegment(辺)とNode(点)を紐づけてネットワークを構築する関数
    /// </summary>

    private void InitializeGraph()
    {
        //1.必要なコンポーネントの取得
        allSegments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();
        allNodes = FindObjectsByType<LaneNode>(FindObjectsSortMode.None).ToList();

        foreach(var seg in allSegments)
        {
            seg.nodeA = FindClosestNode(seg.transform.position);
            seg.nodeB = FindClosestNode(seg.transform.position);

            if (seg.nodeA != null) seg.nodeA.connectedSegments.Add(seg);
            if (seg.nodeB != null) seg.nodeB.connectedSegments.Add(seg);
        }
    }

    private LaneNode FindClosestNode(Vector3 pos) {/*距離計算ロジックをこれから作る*/ }
}
