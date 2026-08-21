using UnityEngine;
/// <summary>
/// LaneSegmentのLaneColorを参照して、
/// 指定した条件（直下、または特定オブジェクト名）のRendererのMaterialを自動設定するコンポーネント。
/// LaneBaseは対象外。
/// </summary>
public class LaneTileColorApplier : MonoBehaviour
{
    public enum TargetMode
    {
        DirectChildrenUnderLane, // Lane直下のすべてのRendererを対象
        MatchNames               // 指定した名前に一致するRendererを対象（階層が深くてもOK）
    }

    [Header("パレット")]
    [SerializeField] public LaneColorPalette palette;

    [Header("Laneオブジェクトの名前")]
    [SerializeField] private string laneChildName = "Lane";

    [Header("変更対象の指定方法")]
    [SerializeField] private TargetMode targetMode = TargetMode.DirectChildrenUnderLane;

    [Header("対象の名前リスト (MatchNamesモード時のみ有効)")]
    [Tooltip("ここに登録した名前（部分一致または完全一致）を持つオブジェクトのMaterialを変更します")]
    [SerializeField] private string[] targetObjectNames = { "Cube", "Cylinder" };

    [Tooltip("完全一致のみにする場合はチェック（オフなら部分一致）")]
    [SerializeField] private bool exactMatch = true;

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

        // 選択されているモードに応じて色を適用
        if (targetMode == TargetMode.DirectChildrenUnderLane)
        {
            // Lane直下のRendererのみ対象（孫以降は含まない）
            foreach (Transform child in laneTransform)
            {
                var rend = child.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = mat;
            }
        }
        else if (targetMode == TargetMode.MatchNames)
        {
            // Laneの子孫すべてから、指定された名前に一致するRendererを探して適用
            ApplyColorRecursive(laneTransform, mat);
        }
    }

    private void ApplyColorRecursive(Transform current, Material mat)
    {
        // 自身の名前がリストに含まれているかチェック
        if (IsTargetNameMatch(current.name))
        {
            var rend = current.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = mat;
            }
        }

        // 子階層を再帰的に探索
        foreach (Transform child in current)
        {
            ApplyColorRecursive(child, mat);
        }
    }

    private bool IsTargetNameMatch(string objName)
    {
        if (targetObjectNames == null || targetObjectNames.Length == 0) return false;

        foreach (var targetName in targetObjectNames)
        {
            if (string.IsNullOrEmpty(targetName)) continue;

            if (exactMatch)
            {
                if (objName == targetName) return true;
            }
            else
            {
                if (objName.Contains(targetName)) return true;
            }
        }
        return false;
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
        if (GUILayout.Button("🔄 Editorで色を適用", GUILayout.Height(30)))
            applier.ApplyInEditor();
        GUI.backgroundColor = Color.white;

        UnityEditor.EditorGUILayout.HelpBox(
            "・DirectChildrenUnderLane: Lane直下のRendererのみ変更\n" +
            "・MatchNames: 指定した名前(複数可)に一致する子孫のRendererを変更",
            UnityEditor.MessageType.Info
        );
    }
}
#endif