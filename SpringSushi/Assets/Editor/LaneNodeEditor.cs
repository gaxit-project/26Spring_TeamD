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

        // --- 出口設定（ObjectField：ドラッグ＆ドロップ対応） ---
        EditorGUILayout.LabelField("出口設定", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("defaultExit"),
            new GUIContent("Default Exit（通常時）", "IsReversed = false のとき寿司が向かう Segment")
        );

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("reversedExit"),
            new GUIContent("Reversed Exit（反転時）", "IsReversed = true のとき寿司が向かう Segment")
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

    // --- Gizmo 描画 ---
    private void OnSceneGUI()
    {
        var node = (LaneNode)target;
        DrawExitArrow(node, node.defaultExit, Color.white, "Default");
        DrawExitArrow(node, node.reversedExit, Color.yellow, "Reversed");
    }

    private void DrawExitArrow(LaneNode node, LaneSegment exit, Color color, string labelPrefix)
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

        Vector3 arrowTip = Vector3.Lerp(from, to, 0.7f);
        Handles.ArrowHandleCap(
            0,
            arrowTip,
            Quaternion.LookRotation(dir.normalized),
            dist * 0.15f,
            EventType.Repaint
        );

        Handles.Label(
            Vector3.Lerp(from, to, 0.5f) + Vector3.up * 0.3f,
            $"{labelPrefix}: {exit.gameObject.name}",
            new GUIStyle { normal = { textColor = color }, fontSize = 11 }
        );
    }
}
#endif