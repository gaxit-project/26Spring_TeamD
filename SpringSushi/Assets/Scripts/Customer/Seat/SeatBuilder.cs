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

        foreach (var anchor in anchors)
        {
            var seat = (GameObject)PrefabUtility.InstantiatePrefab(seatPrefab);
            Undo.RegisterCreatedObjectUndo(seat, "Generate Seat");
            seat.transform.position = anchor.transform.position;
            seat.transform.rotation = anchor.transform.rotation;
            seat.transform.SetParent(anchor.transform);
            seat.name = $"Seat_{anchor.name}";

            customerManager.AddEntryPoint(anchor.transform);
            EditorUtility.SetDirty(customerManager);
        }

        Debug.Log($"[SeatBuilder] {anchors.Length}個の椅子を生成しました。");
    }

    public void ClearSeats()
    {
        if (customerManager == null) return;
        customerManager.entryPoints.Clear();
        EditorUtility.SetDirty(customerManager);
        Debug.Log("[SeatBuilder] EntryPointsをクリアしました。");
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