using UnityEngine;
using UnityEditor;

/// <summary>
/// SeatBuilder のカスタムInspector。
/// このファイルは Assets/Editor/ フォルダに置いてください。
/// </summary>
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
            if (GUILayout.Button("椅子を生成", GUILayout.Height(32)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Generate Seats");
                builder.GenerateSeats();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("クリア", GUILayout.Height(32), GUILayout.Width(80)))
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