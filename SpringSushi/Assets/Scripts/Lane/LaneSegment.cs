using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("設定")]
    public LaneColor laneColor;

    [HideInInspector] public LaneNode nodeA;
    [HideInInspector] public LaneNode nodeB;

    public bool isReversed = false;

    public LaneNode GetExitNode() => isReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => isReversed ? nodeB : nodeA;

    // ★ 追加：状態を直接セット
    public void SetReversed(bool value)
    {
        isReversed = value;

        Debug.Log($"{gameObject.name} の isReversed が {isReversed} になりました！", gameObject);

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isReversed ? Color.black : Color.white;
        }
    }

    // （必要なら残す）
    public void Reverse()
    {
        SetReversed(!isReversed);
    }

    /*
    public LaneSegment GetNextSegment()
    {
        LaneNode startNode = GetExitNode();
        if (startNode == null) return null;

        foreach (var segment in startNode.connectedSegments)
        {
            if (segment == this) continue;

            // 【修正ポイント】
            // 相手の Entry/Exit に関係なく、物理的に繋がっていれば一旦「次」として認める。
            // 進むべき方向は SushiMovement 側が isReversed を見て自動で判断するため、
            // ここでは「道がつながっているか」だけを返せばOKです。
            return segment;
        }
        return null;
    }
    */
    private void OnDrawGizmos()
    {
        LaneNode[] childNodes = GetComponentsInChildren<LaneNode>();
        if (childNodes.Length < 2) return;

        Vector3 posA = childNodes[0].Position;
        Vector3 posB = childNodes[1].Position;

        Gizmos.color = isReversed ? Color.black : Color.white;
        Gizmos.DrawLine(posA, posB);

        Vector3 exitPos = isReversed ? posA : posB;
        Gizmos.DrawSphere(exitPos, 0.15f);
    }
}