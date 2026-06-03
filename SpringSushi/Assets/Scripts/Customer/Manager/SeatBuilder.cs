using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

/// <summary>
/// CustomerManagerにアタッチして使う。
/// Editorボタンで椅子PrefabをEntryPointsに一括生成する。
/// </summary>
public class SeatBuilder : MonoBehaviour
{
    [Header("椅子設定")]
    [SerializeField] private GameObject seatPrefab;
    [SerializeField] private CustomerManager customerManager;

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

        // 既存のEntryPointsをクリア
        ClearSeats();

        // EntryPointsの各位置に椅子を生成
        // ※ EntryPoint用の空GameObjectが事前にシーンに配置されている前提
        //   （SeatAnchorタグで検索）
        var anchors = GameObject.FindGameObjectsWithTag("SeatAnchor");
        if (anchors.Length == 0)
        {
            Debug.LogWarning("[SeatBuilder] 'SeatAnchor'タグのGameObjectが見つかりません。\n椅子を置きたい位置に空GameObjectを置いて'SeatAnchor'タグを付けてください。");
            return;
        }

        // シーン内の全LaneSegmentを収集（検索用）
        var allSegments = Object.FindObjectsByType<LaneSegment>(FindObjectsSortMode.None);

        foreach (var anchor in anchors)
        {
            var seat = (GameObject)PrefabUtility.InstantiatePrefab(seatPrefab);
            Undo.RegisterCreatedObjectUndo(seat, "Generate Seat");
            seat.transform.position = anchor.transform.position;
            seat.transform.rotation = CalcSeatRotation(anchor.transform.position, allSegments);
            seat.transform.SetParent(anchor.transform);
            seat.name = $"Seat_{anchor.name}";

            customerManager.AddEntryPoint(anchor.transform);
            EditorUtility.SetDirty(customerManager);
        }

        Debug.Log($"[SeatBuilder] {anchors.Length}個の椅子を生成しました。");
    }

    /// <summary>
    /// SeatAnchor位置に最も近いLaneSegmentのA→B方向へ
    /// 椅子のZ+を向ける回転を計算する。
    /// </summary>
    private Quaternion CalcSeatRotation(Vector3 anchorPos, LaneSegment[] segments)
    {
        if (segments == null || segments.Length == 0)
            return Quaternion.identity;

        // 最も近いSegmentを検索
        LaneSegment nearest = null;
        float minDist = float.MaxValue;
        foreach (var seg in segments)
        {
            if (seg.nodeA == null || seg.nodeB == null) continue;
            // Segmentの中点との距離で比較
            Vector3 mid = (seg.nodeA.Position + seg.nodeB.Position) * 0.5f;
            float dist = Vector3.Distance(anchorPos, mid);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = seg;
            }
        }

        if (nearest == null) return Quaternion.identity;

        // A→B方向をSegmentの進行方向とし、椅子のZ+をその方向へ向ける
        Vector3 segDir = (nearest.nodeB.Position - nearest.nodeA.Position).normalized;
        if (segDir == Vector3.zero) return Quaternion.identity;

        return Quaternion.LookRotation(segDir, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
    }

    public void ClearSeats()
    {
        // SeatAnchorタグのオブジェクト配下にある椅子を全削除
        var anchors = GameObject.FindGameObjectsWithTag("SeatAnchor");
        int removed = 0;
        foreach (var anchor in anchors)
        {
            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in anchor.transform)
                children.Add(child.gameObject);
            foreach (var child in children)
            {
                Undo.DestroyObjectImmediate(child);
                removed++;
            }
        }

        // EntryPointsリストもクリア
        if (customerManager != null)
        {
            customerManager.entryPoints.Clear();
            EditorUtility.SetDirty(customerManager);
        }

        Debug.Log($"[SeatBuilder] {removed}個の椅子を削除しました。");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SeatBuilder))]
public class SeatBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        var builder = (SeatBuilder)target;

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.5f);
            if (GUILayout.Button("? 椅子を生成", GUILayout.Height(32)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Generate Seats");
                builder.GenerateSeats();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("? クリア", GUILayout.Height(32), GUILayout.Width(80)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Clear Seats");
                builder.ClearSeats();
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.HelpBox(
            "椅子を置きたい位置に空のGameObjectを置き、タグを 'SeatAnchor' に設定してください。\n" +
            "「椅子を生成」ボタンを押すとSeatAnchorの位置に椅子Prefabが生成され、EntryPointsに自動登録されます。",
            MessageType.Info
        );
    }
}
#endif