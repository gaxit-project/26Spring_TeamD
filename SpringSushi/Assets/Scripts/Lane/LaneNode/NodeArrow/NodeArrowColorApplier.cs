using UnityEngine;

/// <summary>
/// LaneNodeのLaneColorを参照して、Node直下のCylinderのMaterialを設定する。
/// NodeBase は対象外。
///
/// 【Hierarchy構成】
/// LaneNode
///   └─ NodeArrow (NodeArrowColorApplier.cs) ← ここにアタッチ
///        ├─ NodeBase  ← 対象外
///        └─ Node
///             └─ Cylinder ← Material変更対象
/// </summary>
public class NodeArrowColorApplier : MonoBehaviour
{
    [Header("パレット")]
    public LaneColorPalette palette;

    [Tooltip("色を適用する子オブジェクトの名前（この直下のRendererのみ対象）")]
    [SerializeField] private string nodeChildName = "Node";

    private LaneNode node;

    private void Awake()
    {
        node = GetComponentInParent<LaneNode>();
        if (node == null)
        {
            Debug.LogWarning($"[NodeArrowColorApplier] {gameObject.name}: 親にLaneNodeが見つかりません。");
            return;
        }
        ApplyColor(node.laneColor);
    }

    public void ApplyColor(LaneColor color)
    {
        if (palette == null)
        {
            Debug.LogWarning($"[NodeArrowColorApplier] {gameObject.name}: paletteが未アサインです。");
            return;
        }

        Material mat = palette.GetMaterial(color);
        if (mat == null)
        {
            Debug.LogWarning($"[NodeArrowColorApplier] {gameObject.name}: LaneColor.{color} がパレットに未登録です。");
            return;
        }

        Transform nodeTransform = FindChildByName(transform, nodeChildName);
        if (nodeTransform == null)
        {
            Debug.LogWarning($"[NodeArrowColorApplier] {gameObject.name}: '{nodeChildName}' が見つかりません。");
            return;
        }

        // Node直下のRendererのみ対象
        foreach (Transform child in nodeTransform)
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
        node = GetComponentInParent<LaneNode>();
        if (node == null || palette == null) return;
        ApplyColor(node.laneColor);
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(NodeArrowColorApplier))]
public class NodeArrowColorApplierEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.Space(8);
        var applier = (NodeArrowColorApplier)target;
        GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
        if (GUILayout.Button("? Editorで色を適用", GUILayout.Height(30)))
            applier.ApplyInEditor();
        GUI.backgroundColor = Color.white;
        UnityEditor.EditorGUILayout.HelpBox(
            "Node直下（NodeBase除く）のRendererのみMaterialを変更します。",
            UnityEditor.MessageType.Info
        );
    }
}
#endif