using UnityEngine;

public class LaneNodeView : MonoBehaviour
{
    [SerializeField] private LaneNode node;

    [Header("回転設定")]
    [SerializeField] private Transform arrowPlate;
    [Tooltip("回転をなめらかにする速さ。0にするとスナップ回転")]
    [SerializeField] private float rotateSpeed = 10f;

    private Quaternion targetRotation;

    private void Awake()
    {
        if (node == null)
            node = GetComponentInParent<LaneNode>();

        if (arrowPlate == null)
            arrowPlate = transform;

        node.OnExitIndexChanged += OnExitIndexChanged;

        UpdateTargetRotation();
        arrowPlate.rotation = targetRotation;
    }

    private void Update()
    {
        if (rotateSpeed <= 0f)
            arrowPlate.rotation = targetRotation;
        else
            arrowPlate.rotation = Quaternion.Slerp(arrowPlate.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void OnExitIndexChanged(int index)
    {
        UpdateTargetRotation();
    }

    private void UpdateTargetRotation()
    {
        LaneSegment exitSeg = node.GetExitSegment();
        if (exitSeg == null) return;

        Vector3 dir = GetExitDirection(exitSeg);
        if (dir == Vector3.zero) return;

        targetRotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    private Vector3 GetExitDirection(LaneSegment seg)
    {
        if (seg.nodeA == null || seg.nodeB == null) return Vector3.zero;

        if (seg.nodeA == node)
            return (seg.nodeB.Position - seg.nodeA.Position).normalized;
        else if (seg.nodeB == node)
            return (seg.nodeA.Position - seg.nodeB.Position).normalized;

        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (node != null)
            node.OnExitIndexChanged -= OnExitIndexChanged;
    }
}