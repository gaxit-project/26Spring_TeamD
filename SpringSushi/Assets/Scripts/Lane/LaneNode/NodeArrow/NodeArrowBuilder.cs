using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// LaneNodeにNodeArrowPrefabを1つ配置するコンポーネント。
/// LaneTileBuilderと同様にEditorボタンで生成・クリアを実行する。
///
/// 【Hierarchy構成（生成後）】
/// LaneNode (NodeArrowBuilder.cs)
///   └─ NodeArrow (NodeArrowColorApplier.cs, NodeArrowRotator.cs)
///        ├─ NodeBase
///        ├─ Node
///        │   └─ Cylinder
///        └─ Arrow
/// </summary>
public class NodeArrowBuilder : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LaneNode node;

    [Header("Prefab設定")]
    [SerializeField] private GameObject nodeArrowPrefab;

    [Header("色設定")]
    [SerializeField] private LaneColorPalette colorPalette;

    [Header("配置オフセット")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    private void Reset()
    {
        node = GetComponent<LaneNode>();
    }

#if UNITY_EDITOR
    public void GenerateArrow()
    {
        if (node == null)
        {
            Debug.LogWarning("[NodeArrowBuilder] LaneNodeが未アサインです。");
            return;
        }
        if (nodeArrowPrefab == null)
        {
            Debug.LogWarning("[NodeArrowBuilder] nodeArrowPrefabが未アサインです。");
            return;
        }

        ClearArrow();

        GameObject arrow = (GameObject)PrefabUtility.InstantiatePrefab(nodeArrowPrefab, transform);
        arrow.transform.position = node.Position + positionOffset;
        arrow.transform.rotation = Quaternion.identity;
        arrow.name = "NodeArrow";

        // 色を適用
        if (colorPalette != null)
        {
            var colorApplier = arrow.GetComponent<NodeArrowColorApplier>();
            if (colorApplier != null)
            {
                colorApplier.palette = colorPalette;
                colorApplier.ApplyInEditor();
            }
        }

        Debug.Log($"[NodeArrowBuilder] {gameObject.name}: NodeArrowを生成しました。");
        EditorUtility.SetDirty(gameObject);
    }

    public void ClearArrow()
    {
        // "NodeArrow"という名前の子を全削除
        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.name == "NodeArrow")
                toDelete.Add(child.gameObject);
        }
        foreach (var obj in toDelete)
            DestroyImmediate(obj);

        EditorUtility.SetDirty(gameObject);
    }

    public void ApplyColor()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "NodeArrow") continue;
            var applier = child.GetComponent<NodeArrowColorApplier>();
            if (applier == null) continue;
            if (applier.palette == null)
                applier.palette = colorPalette;
            applier.ApplyInEditor();
        }
        Debug.Log($"[NodeArrowBuilder] {gameObject.name}: 色を適用しました。");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(NodeArrowBuilder))]
public class NodeArrowBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        var builder = (NodeArrowBuilder)target;

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.5f);
            if (GUILayout.Button("? 生成", GUILayout.Height(32)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Generate NodeArrow");
                builder.GenerateArrow();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("? クリア", GUILayout.Height(32), GUILayout.Width(80)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Clear NodeArrow");
                builder.ClearArrow();
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
        if (GUILayout.Button("?? 色を適用", GUILayout.Height(28)))
        {
            Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Apply NodeArrow Color");
            builder.ApplyColor();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "LaneNodeの位置にNodeArrowを1つ配置します。\n" +
            "colorPaletteが設定されていれば生成時に色も自動適用されます。\n" +
            "Ctrl+Z でUndo可能です。",
            MessageType.Info
        );
    }
}
#endif