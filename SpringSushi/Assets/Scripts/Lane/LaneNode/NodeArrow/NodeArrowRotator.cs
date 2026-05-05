using UnityEngine;

public class NodeArrowRotator : MonoBehaviour
{
    [SerializeField] private LaneNode node;
    [SerializeField] private Transform arrowTransform;

    private void Awake()
    {
        if (node == null)
            node = GetComponentInParent<LaneNode>();

        if (node == null)
        {
            Debug.LogWarning($"[NodeArrowRotator] {gameObject.name}: êeÇ…LaneNodeÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            return;
        }

        if (arrowTransform == null)
            arrowTransform = FindChildByName(transform, "Arrow");

        if (arrowTransform == null)
        {
            Debug.LogWarning($"[NodeArrowRotator] {gameObject.name}: 'Arrow'Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            return;
        }

        node.OnExitIndexChanged += OnExitIndexChanged;
        ApplyDirection();
    }

    private void OnExitIndexChanged(int index)
    {
        ApplyDirection();
    }

    private void ApplyDirection()
    {
        if (arrowTransform == null || node == null) return;

        LaneSegment exitSeg = node.GetExitSegment();
        if (exitSeg == null) return;

        Vector3 dir = GetExitDirection(exitSeg);
        if (dir == Vector3.zero) return;

        arrowTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    private Vector3 GetExitDirection(LaneSegment seg)
    {
        if (seg.nodeA == null || seg.nodeB == null) return Vector3.zero;

        if (seg.nodeA == node)
            return (seg.nodeB.Position - seg.nodeA.Position).normalized;
        else if (seg.nodeB == node)
            return (seg.nodeA.Position - seg.nodeB.Position).normalized;

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
            node.OnExitIndexChanged -= OnExitIndexChanged;
    }
}