using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CustomerManagerにアタッチして使う。
/// Editorボタンで椅子PrefabをEntryPointsに一括生成する。
/// </summary>
public class SeatBuilder : MonoBehaviour
{
    [Header("椅子設定")]
    [SerializeField] private GameObject seatPrefab;
    [SerializeField] private CustomerManager customerManager;
    public GameObject SeatPrefab => seatPrefab;
    public CustomerManager CustomerManager => customerManager;

    private void Reset()
    {
        customerManager = GetComponent<CustomerManager>();
    }

#if UNITY_EDITOR
    public void GenerateSeats()
    {
        if (seatPrefab == null)
        {
            Debug.LogWarning("[SeatBuilder] seatPrefabが未設定です。");
            return;
        }
        if (customerManager == null)
        {
            Debug.LogWarning("[SeatBuilder] CustomerManagerが未設定です。");
            return;
        }
        ClearSeats();
        var anchors = GameObject.FindGameObjectsWithTag("SeatAnchor");
        if (anchors.Length == 0)
        {
            Debug.LogWarning("[SeatBuilder] 'SeatAnchor'タグのGameObjectが見つかりません。");
            return;
        }
        var allSegments = UnityEngine.Object.FindObjectsByType<LaneSegment>(FindObjectsSortMode.None);
        foreach (var anchor in anchors)
        {
            var seat = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(seatPrefab);
            UnityEditor.Undo.RegisterCreatedObjectUndo(seat, "Generate Seat");
            seat.transform.position = anchor.transform.position;
            seat.transform.rotation = CalcSeatRotation(anchor.transform.position, allSegments);
            seat.transform.SetParent(anchor.transform);
            seat.name = $"Seat_{anchor.name}";
            customerManager.AddEntryPoint(anchor.transform);
            UnityEditor.EditorUtility.SetDirty(customerManager);
        }
        Debug.Log($"[SeatBuilder] {anchors.Length}個の椅子を生成しました。");
    }

    /// <summary>
    /// anchorPosに最も近いレーン区間を見つけ、
    /// 「レーン上の最近接点から見てanchorがある側」を椅子の正面にする。
    /// 区間のnodeA/nodeBの登録順序に依存しないため、
    /// 複雑な形状のレーンでも椅子が逆向きになることがない。
    /// </summary>
    private Quaternion CalcSeatRotation(Vector3 anchorPos, LaneSegment[] segments)
    {
        if (segments == null || segments.Length == 0)
            return Quaternion.identity;

        LaneSegment nearest = null;
        Vector3 nearestPointOnSegment = Vector3.zero;
        float minDist = float.MaxValue;

        foreach (var seg in segments)
        {
            if (seg.nodeA == null || seg.nodeB == null) continue;

            Vector3 a = seg.nodeA.Position;
            Vector3 b = seg.nodeB.Position;
            Vector3 closest = ClosestPointOnSegment(anchorPos, a, b);
            float dist = Vector3.Distance(anchorPos, closest);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = seg;
                nearestPointOnSegment = closest;
            }
        }

        if (nearest == null) return Quaternion.identity;

        Vector3 segDir = (nearest.nodeB.Position - nearest.nodeA.Position).normalized;
        if (segDir == Vector3.zero) return Quaternion.identity;

        // レーン上の最近接点から、椅子(anchor)へ向かうベクトル = 「客席側」を指す方向
        Vector3 toAnchor = anchorPos - nearestPointOnSegment;
        toAnchor.y = 0f; // 水平方向のみで判定する

        // segDirに対して垂直な方向(左右どちらか)を候補とし、
        // 実際にanchorがある側(客席側)を選ぶ
        Vector3 sideDir = Vector3.Cross(Vector3.up, segDir);
        float side = Vector3.Dot(toAnchor, sideDir);

        Vector3 facing = side >= 0f ? sideDir : -sideDir;

        return Quaternion.LookRotation(facing, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
    }

    /// <summary>
    /// 線分a-b上で、点pに最も近い点を求める。
    /// </summary>
    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f);
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    public void ClearSeats()
    {
        var anchors = GameObject.FindGameObjectsWithTag("SeatAnchor");
        int removed = 0;
        foreach (var anchor in anchors)
        {
            var children = new List<GameObject>();
            foreach (Transform child in anchor.transform)
                children.Add(child.gameObject);
            foreach (var child in children)
            {
                UnityEditor.Undo.DestroyObjectImmediate(child);
                removed++;
            }
        }
        if (customerManager != null)
        {
            customerManager.entryPoints.Clear();
            UnityEditor.EditorUtility.SetDirty(customerManager);
        }
        Debug.Log($"[SeatBuilder] {removed}個の椅子を削除しました。");
    }
#endif
}