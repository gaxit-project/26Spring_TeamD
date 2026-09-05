using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// シーン内の全LaneSegment/LaneNodeに対して、
/// 子孫にある"SegmentSelectFlame"/"NodeSelectFlame"という名前のオブジェクトを
/// 自動検索し、SelectFlameフィールドへ一括で割り当てるEditor拡張。
/// </summary>
public class LaneGlowAutoAssigner : MonoBehaviour
{
#if UNITY_EDITOR
    private const string SegmentFlameName = "SegmentSelectFlame";
    private const string NodeFlameName = "NodeSelectFlame";

    /// <summary>
    /// シーン内の全LaneSegmentに対して、SegmentSelectFlameを自動検索して割り当てる。
    /// </summary>
    public static void AssignAllSegmentFlames()
    {
        var segments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None);
        int assigned = 0;
        int skipped = 0;

        foreach (var seg in segments)
        {
            var flame = FindDescendantByName(seg.transform, SegmentFlameName);
            if (flame == null)
            {
                Debug.LogWarning($"[LaneGlowAutoAssigner] {seg.gameObject.name}: '{SegmentFlameName}' が見つかりませんでした。");
                skipped++;
                continue;
            }

            var so = new SerializedObject(seg);
            var prop = so.FindProperty("selectFlame");
            prop.objectReferenceValue = flame.gameObject;
            so.ApplyModifiedProperties();

            assigned++;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Segment: {assigned}件 割り当て完了 / {skipped}件 見つからず");
    }

    /// <summary>
    /// シーン内の全LaneNodeに対して、NodeSelectFlameを自動検索して割り当てる。
    /// </summary>
    public static void AssignAllNodeFlames()
    {
        var nodes = FindObjectsByType<LaneNode>(FindObjectsSortMode.None);
        int assigned = 0;
        int skipped = 0;

        foreach (var node in nodes)
        {
            var flame = FindDescendantByName(node.transform, NodeFlameName);
            if (flame == null)
            {
                Debug.LogWarning($"[LaneGlowAutoAssigner] {node.gameObject.name}: '{NodeFlameName}' が見つかりませんでした。");
                skipped++;
                continue;
            }

            var so = new SerializedObject(node);
            var prop = so.FindProperty("selectFlame");
            prop.objectReferenceValue = flame.gameObject;
            so.ApplyModifiedProperties();

            assigned++;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Node: {assigned}件 割り当て完了 / {skipped}件 見つからず");
    }

    /// <summary>
    /// 指定した名前を持つ子孫のTransformを、階層をどれだけ深く辿っても検索する。
    /// </summary>
    private static Transform FindDescendantByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDescendantByName(child, name);
            if (result != null) return result;
        }
        return null;
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// メニューバーから実行できるようにするEditor拡張。
/// </summary>
public static class LaneGlowAutoAssignerMenu
{
    [MenuItem("Tools/Lane/Assign All Segment Flames")]
    private static void AssignSegmentFlames()
    {
        LaneGlowAutoAssigner.AssignAllSegmentFlames();
    }

    [MenuItem("Tools/Lane/Assign All Node Flames")]
    private static void AssignNodeFlames()
    {
        LaneGlowAutoAssigner.AssignAllNodeFlames();
    }

    [MenuItem("Tools/Lane/Assign All Flames (Segment + Node)")]
    private static void AssignAllFlames()
    {
        LaneGlowAutoAssigner.AssignAllSegmentFlames();
        LaneGlowAutoAssigner.AssignAllNodeFlames();
    }
}
#endif