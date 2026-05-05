#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LaneNode))]
public class LaneNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var node = (LaneNode)target;
        serializedObject.Update();

        // --- レーン設定 ---
        EditorGUILayout.LabelField("レーン設定", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("laneColor"));

        EditorGUILayout.Space(8);

        // --- 出口設定（リスト：D&D対応） ---
        EditorGUILayout.LabelField("出口設定（ボタンを押すたびに順番に切り替わる）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("exitSegments"),
            new GUIContent("Exit Segments", "ボタンを押すたびにelement0→1→2→...→0と循環する"),
            true
        );

        EditorGUILayout.Space(8);

        // --- 接続情報（読み取り専用） ---
        EditorGUILayout.LabelField("接続 Segment（実行時に自動収集）", EditorStyles.boldLabel);
        if (node.connectedSegments.Count == 0)
        {
            EditorGUILayout.HelpBox("実行中に LaneGraphBuilder が自動設定します。", MessageType.Info);
        }
        else
        {
            GUI.enabled = false;
            foreach (var seg in node.connectedSegments)
                EditorGUILayout.ObjectField(seg, typeof(LaneSegment), true);
            GUI.enabled = true;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // --- Gizmo描画 ---
    private void OnSceneGUI()
    {
        var node = (LaneNode)target;
        if (node.exitSegments == null) return;

        // 全出口を色分けして表示
        for (int i = 0; i < node.exitSegments.Count; i++)
        {
            // 現在選択中 → 白、それ以外 → グレー
            Color col = (i == node.CurrentExitIndex) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.5f);
            DrawExitArrow(node, node.exitSegments[i], col, $"[{i}]");
        }
    }

    private void DrawExitArrow(LaneNode node, LaneSegment exit, Color color, string label)
    {
        if (exit == null) return;

        LaneNode otherNode = null;
        if (exit.nodeA != null && exit.nodeA != node) otherNode = exit.nodeA;
        else if (exit.nodeB != null && exit.nodeB != node) otherNode = exit.nodeB;

        Vector3 from = node.transform.position;
        Vector3 to = otherNode != null ? otherNode.transform.position : exit.transform.position;
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.001f) return;

        Handles.color = color;
        Handles.DrawLine(from, to, 3f);

        Handles.ArrowHandleCap(
            0,
            Vector3.Lerp(from, to, 0.7f),
            Quaternion.LookRotation(dir.normalized),
            dist * 0.15f,
            EventType.Repaint
        );

        Handles.Label(
            Vector3.Lerp(from, to, 0.5f) + Vector3.up * 0.3f,
            $"{label}: {exit.gameObject.name}",
            new GUIStyle { normal = { textColor = color }, fontSize = 11 }
        );
    }
}
#endif