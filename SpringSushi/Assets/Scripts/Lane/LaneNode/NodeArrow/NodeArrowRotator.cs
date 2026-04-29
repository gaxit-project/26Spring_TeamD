using UnityEngine;

/// <summary>
/// LaneNodeのIsReversed変化を受けて、
/// ArrowをdefaultExit/reversedExitの方向に向かせる。
/// ArrowのローカルZ+が先端方向。
///
/// 【Hierarchy構成】
/// LaneNode
///   └─ NodeArrow (NodeArrowRotator.cs) ← ここにアタッチ
///        ├─ NodeBase
///        ├─ Node
///        └─ Arrow ← Z+が先端。出口方向に向く
/// </summary>
public class NodeArrowRotator : MonoBehaviour
{
    [SerializeField] private LaneNode node;

    [Tooltip("回転させるArrowオブジェクト。nullの場合'Arrow'という名前の子を自動検索する")]
    [SerializeField] private Transform arrowTransform;

    private void Awake()
    {
        if (node == null)
            node = GetComponentInParent<LaneNode>();

        if (node == null)
        {
            Debug.LogWarning($"[NodeArrowRotator] {gameObject.name}: 親にLaneNodeが見つかりません。");
            return;
        }

        if (arrowTransform == null)
            arrowTransform = FindChildByName(transform, "Arrow");

        if (arrowTransform == null)
        {
            Debug.LogWarning($"[NodeArrowRotator] {gameObject.name}: 'Arrow'という名前の子が見つかりません。");
            return;
        }

        node.OnReversedChanged += OnReversedChanged;
        ApplyDirection(node.IsReversed);
    }

    private void OnReversedChanged(bool isReversed)
    {
        ApplyDirection(isReversed);
    }

    private void ApplyDirection(bool isReversed)
    {
        if (arrowTransform == null || node == null) return;

        LaneSegment exitSeg = isReversed ? node.reversedExit : node.defaultExit;
        if (exitSeg == null) return;

        Vector3 dir = GetExitDirection(exitSeg);
        if (dir == Vector3.zero) return;

        // ArrowのZ+を出口方向に向ける
        arrowTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    private Vector3 GetExitDirection(LaneSegment seg)
    {
        if (seg.nodeA == null || seg.nodeB == null) return Vector3.zero;

        // このNodeから出口Segmentへ進む方向を計算
        if (seg.nodeA == node)
            return (seg.nodeB.Position - seg.nodeA.Position).normalized;
        else if (seg.nodeB == node)
            return (seg.nodeA.Position - seg.nodeB.Position).normalized;

        // このNodeがSegmentの端点でない場合（中継など）はSegmentの中心方向で代替
        Vector3 segCenter = (seg.nodeA.Position + seg.nodeB.Position) * 0.5f;
        return (segCenter - node.Position).normalized;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void OnDestroy()
    {
        if (node != null)
            node.OnReversedChanged -= OnReversedChanged;
    }
}