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

    public string FlowDirectionName => isReversed ? $"{nodeB.name} → {nodeA.name}" : $"{nodeA.name} → {nodeB.name}";
    // ★ 追加：状態を直接セット
    public void SetReversed(bool value)
    {
        isReversed = value;

        // ★ ここで「レーン自体がどちらに動いているか」をログ出力
        Debug.Log($"<color=white>【Lane Update】</color> {gameObject.name} の流れ: <b>{FlowDirectionName}</b>");

        // 見た目の更新
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
        if (nodeA == null || nodeB == null) return;

        Vector3 from = isReversed ? nodeB.Position : nodeA.Position;
        Vector3 to = isReversed ? nodeA.Position : nodeB.Position;
        Vector3 dir = (to - from).normalized;

        Gizmos.color = isReversed ? Color.red : Color.cyan;
        Gizmos.DrawLine(from, to);

        // 先端に小さな線を描いて矢印にする
        float arrowHeadLength = 0.3f;
        float arrowHeadAngle = 20f;
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Gizmos.DrawRay(to, right * arrowHeadLength);
        Gizmos.DrawRay(to, left * arrowHeadLength);
    }
}