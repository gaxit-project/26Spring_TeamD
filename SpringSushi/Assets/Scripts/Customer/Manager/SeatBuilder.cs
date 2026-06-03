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

    private Quaternion CalcSeatRotation(Vector3 anchorPos, LaneSegment[] segments)
    {
        if (segments == null || segments.Length == 0)
            return Quaternion.identity;

        LaneSegment nearest = null;
        float minDist = float.MaxValue;
        foreach (var seg in segments)
        {
            if (seg.nodeA == null || seg.nodeB == null) continue;
            Vector3 mid = (seg.nodeA.Position + seg.nodeB.Position) * 0.5f;
            float dist = Vector3.Distance(anchorPos, mid);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = seg;
            }
        }

        if (nearest == null) return Quaternion.identity;

        Vector3 segDir = (nearest.nodeB.Position - nearest.nodeA.Position).normalized;
        if (segDir == Vector3.zero) return Quaternion.identity;

        return Quaternion.LookRotation(segDir, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
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