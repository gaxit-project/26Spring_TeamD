using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// LaneSegmentのnodeA→nodeB間にCube(1x1x1)を自動配列するコンポーネント。
/// Editorのボタンで生成・クリアを実行する。
/// 生成したCubeはこのGameObjectの子として配置される。
/// </summary>
public class LaneTileBuilder : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LaneSegment segment;

    [Header("タイル設定")]
    [SerializeField] private GameObject tilePrefab;
    [Tooltip("タイルの幅（nodeA-nodeB方向）。通常はPrefabのサイズと合わせて1にする")]
    [SerializeField] private float tileSize = 1f;
    [Tooltip("タイルの親となるTransform。nullの場合このGameObject自身の子になる")]
    [SerializeField] private Transform tileRoot;

    [Header("色設定")]
    [Tooltip("タイル生成時に自動で色を適用するパレット。nullの場合は色適用をスキップする")]
    [SerializeField] private LaneColorPalette colorPalette;

    [Header("生成情報（読み取り専用）")]
    [SerializeField, HideInInspector] private int lastGeneratedCount = 0;

    private void Reset()
    {
        segment = GetComponent<LaneSegment>();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editorから呼ばれる。タイルを生成する。
    /// </summary>
    public void GenerateTiles()
    {
        if (segment == null)
        {
            Debug.LogWarning("[LaneTileBuilder] LaneSegmentが未アサインです。");
            return;
        }
        if (segment.nodeA == null || segment.nodeB == null)
        {
            Debug.LogWarning("[LaneTileBuilder] nodeA または nodeB が未アサインです。");
            return;
        }
        if (tilePrefab == null)
        {
            Debug.LogWarning("[LaneTileBuilder] tilePrefab が未アサインです。");
            return;
        }

        ClearTiles();

        Transform root = tileRoot != null ? tileRoot : transform;
        Vector3 from = segment.nodeA.Position;
        Vector3 to   = segment.nodeB.Position;
        float distance = Vector3.Distance(from, to);
        int count = Mathf.Max(1, Mathf.RoundToInt(distance / tileSize));
        Vector3 dir = (to - from).normalized;
        // PrefabがY軸-90度ずれているため補正
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
        Vector3 yOffset = new Vector3(0f, -0.7f, 0f);

        for (int i = 0; i < count; i++)
        {
            // 各タイルの中心位置：fromからtileSize*0.5オフセットしてi個分進む
            Vector3 pos = from + dir * (i * tileSize + tileSize * 0.5f) + yOffset;
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, root);
            tile.transform.position = pos;
            tile.transform.rotation = rot;
            tile.name = $"Tile_{i:D2}";
        }

        lastGeneratedCount = count;
        Debug.Log($"[LaneTileBuilder] {gameObject.name}: {count}個のタイルを生成しました（距離 {distance:F2}）");

        // 色を一括適用
        if (colorPalette != null)
            ApplyColorToAllTiles();
        else
            Debug.Log("[LaneTileBuilder] colorPaletteが未設定のため色適用をスキップしました。");

        EditorUtility.SetDirty(gameObject);
    }

    /// <summary>
    /// 生成済みのタイルを全削除する。
    /// </summary>
    public void ClearTiles()
    {
        Transform root = tileRoot != null ? tileRoot : transform;
        var children = new List<GameObject>();
        foreach (Transform child in root)
            children.Add(child.gameObject);

        foreach (var child in children)
            DestroyImmediate(child);

        lastGeneratedCount = 0;
        EditorUtility.SetDirty(gameObject);
    }

    /// <summary>
    /// 生成済みの全タイルのApplyInEditor()を呼んで色を一括適用する。
    /// </summary>
    public void ApplyColorToAllTiles()
    {
        Transform root = tileRoot != null ? tileRoot : transform;
        int applied = 0;
        foreach (Transform child in root)
        {
            var applier = child.GetComponent<LaneTileColorApplier>();
            if (applier == null) continue;
            // paletteが未設定のタイルにはBuilderのpaletteを自動セット
            if (applier.palette == null)
                applier.palette = colorPalette;
            applier.ApplyInEditor();
            applied++;
        }
        Debug.Log($"[LaneTileBuilder] {gameObject.name}: {applied}個のタイルに色を適用しました。");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(LaneTileBuilder))]
public class LaneTileBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        var builder = (LaneTileBuilder)target;

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.5f);
            if (GUILayout.Button("? タイルを生成", GUILayout.Height(32)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Generate Lane Tiles");
                builder.GenerateTiles();
            }

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("? クリア", GUILayout.Height(32), GUILayout.Width(80)))
            {
                Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Clear Lane Tiles");
                builder.ClearTiles();
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
        if (GUILayout.Button("?? 色を一括適用", GUILayout.Height(28)))
        {
            Undo.RegisterFullObjectHierarchyUndo(builder.gameObject, "Apply Lane Colors");
            builder.ApplyColorToAllTiles();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "nodeA・nodeB間の距離を tileSize で割った数だけ Prefab を配置します。\n" +
            "タイル生成時にcolorPaletteが設定されていれば色も自動適用されます。\n" +
            "Ctrl+Z でUndo可能です。",
            MessageType.Info
        );
    }
}
#endif