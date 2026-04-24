using UnityEngine;

public class LaneSegmentGizmo : MonoBehaviour
{
    [SerializeField] private LaneSegment segment;

    private void OnDrawGizmos()
    {
        if (segment == null)
            segment = GetComponent<LaneSegment>();

        if (segment.nodeA == null || segment.nodeB == null) return;

        Vector3 from = segment.IsReversed ? segment.nodeB.Position : segment.nodeA.Position;
        Vector3 to = segment.IsReversed ? segment.nodeA.Position : segment.nodeB.Position;
        Vector3 dir = (to - from).normalized;

        Gizmos.color = segment.IsReversed ? Color.red : Color.cyan;
        Gizmos.DrawLine(from, to);

        float arrowHeadLength = 0.3f;
        float arrowHeadAngle = 20f;

        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

        Gizmos.DrawRay(to, right * arrowHeadLength);
        Gizmos.DrawRay(to, left * arrowHeadLength);
    }
}