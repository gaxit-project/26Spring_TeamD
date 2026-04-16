using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("設定")]
    public LaneColor laneColor;

    // 実行時にNetworkによって「代表」として選ばれたNodeが割り当てられる
    [HideInInspector] public LaneNode nodeA;
    [HideInInspector] public LaneNode nodeB;

    public bool isReversed = false;

    // runtimeのNodeを参照するように変更
    public LaneNode GetExitNode() => isReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => isReversed ? nodeB : nodeA;

    public void Reverse()
    {
        isReversed = !isReversed;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isReversed ? Color.black : Color.white;
        }
    }

    public LaneSegment GetNextSegment()
    {
        LaneNode exitNode = GetExitNode();
        if (exitNode == null) return null;

        foreach (var segment in exitNode.connectedSegments)
        {
            if (segment == this) continue;
            // そのセグメントにとって、今の出口が「入口」になっているかを確認
            if (segment.GetEntryNode() == exitNode) return segment;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        // runtimeNodeが割り当てられていない時（エディタ時）は子オブジェクトの座標を使用
        // 子に2つLaneNodeがある想定
        LaneNode[] childNodes = GetComponentsInChildren<LaneNode>();
        if (childNodes.Length < 2) return;

        Vector3 posA = childNodes[0].Position;
        Vector3 posB = childNodes[1].Position;

        Gizmos.color = isReversed ? Color.black : Color.white;
        Gizmos.DrawLine(posA, posB);

        // 出口側に球体
        Vector3 exitPos = isReversed ? posA : posB;
        Gizmos.DrawSphere(exitPos, 0.15f);
    }
}