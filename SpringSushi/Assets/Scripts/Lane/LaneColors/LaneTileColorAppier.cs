using UnityEngine;

/// <summary>
/// LaneSegmentのLaneColorを参照して、
/// Lane直下のCube・CylinderのMaterialを自動設定するコンポーネント。
/// LaneBaseは対象外。
///
/// 【Hierarchy構成】
/// LaneSegment (LaneSegment.cs)
///   └─ Tile_00 (LaneTileColorApplier.cs) ← ここにアタッチ
///        ├─ LaneBase  ← 対象外
///        └─ Lane
///             ├─ Cube      ← Material変更対象
///             └─ Cylinder  ← Material変更対象
/// </summary>
public class LaneTileColorApplier : MonoBehaviour
{
    [Header("パレット")]
    [SerializeField] public LaneColorPalette palette;

    [Header("Laneオブジェクトの名前（この直下のRendererのみ対象）")]
    [SerializeField] private string laneChildName = "Lane";

    private LaneSegment segment;

    private void Awake()
    {
        segment = GetComponentInParent<LaneSegment>();
        if (segment == null)
        {
            UnityEngine.Debug.LogWarning($"[LaneTileColorApplier] {gameObject.name}: 親にLaneSegmentが見つかりません。");
            return;
        }
        ApplyColor(segment.laneColor);
    }

    public void ApplyColor(LaneColor color)
    {
        if (palette == null)
        {
            UnityEngine.Debug.LogWarning($"[LaneTileColorApplier] {gameObject.name}: LaneColorPaletteが未アサインです。");
            return;
        }

        Material mat = palette.GetMaterial(color);
        if (mat == null)
        {
            UnityEngine.Debug.LogWarning($"[LaneTileColorApplier] {gameObject.name}: LaneColor.{color} に対応するMaterialがパレットに未登録です。");
            return;
        }

        // "Lane"という名前の子Transformを探す
        Transform laneTransform = FindChildByName(transform, laneChildName);
        if (laneTransform == null)
        {
            UnityEngine.Debug.LogWarning($"[LaneTileColorApplier] {gameObject.name}: '{laneChildName}' という名前の子が見つかりません。");
            return;
        }

        // Lane直下のRendererのみ対象（孫以降は含まない）
        foreach (Transform child in laneTransform)
        {
            var rend = child.GetComponent<Renderer>();
            if (rend != null)
                rend.material = mat;
        }
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }

#if UNITY_EDITOR
    public void ApplyInEditor()
    {
        segment = GetComponentInParent<LaneSegment>();
        if (segment == null || palette == null) return;
        ApplyColor(segment.laneColor);
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(LaneTileColorApplier))]
public class LaneTileColorApplierEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.Space(8);

        var applier = (LaneTileColorApplier)target;
        GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
        if (GUILayout.Button("? Editorで色を適用", GUILayout.Height(30)))
            applier.ApplyInEditor();
        GUI.backgroundColor = Color.white;

        UnityEditor.EditorGUILayout.HelpBox(
            "Lane直下（LaneBase除く）のRendererのみMaterialを変更します。\n実行時はAwake()で自動適用されます。",
            UnityEditor.MessageType.Info
        );
    }
}
#endif