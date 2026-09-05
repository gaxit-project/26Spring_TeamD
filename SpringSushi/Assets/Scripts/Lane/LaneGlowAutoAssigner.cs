using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LaneGlowAutoAssigner : MonoBehaviour
{
#if UNITY_EDITOR
    private const string SegmentFlameName = "SegmentSelectFlame";
    private const string NodeFlameName = "NodeSelectFlame";

    /// <summary>
    /// シーン内の全LaneSegmentに対して、配下の全タイルのSegmentSelectFlameを検索して割り当てる
    /// </summary>
    public static void AssignAllSegmentFlames()
    {
        var segments = FindObjectsByType<LaneSegment>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int totalFlameCount = 0;

        foreach (var seg in segments)
        {
            var flames = new List<GameObject>();
            FindAllDescendantsByName(seg.transform, SegmentFlameName, flames);

            seg.EditorSetSelectFlames(flames);
            EditorUtility.SetDirty(seg);
            EditorSceneManager.MarkSceneDirty(seg.gameObject.scene);

            totalFlameCount += flames.Count;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Segment: {segments.Length}区間 (合計 {totalFlameCount}個のFlame) 割り当て完了");
    }

    /// <summary>
    /// シーン内の全LaneNodeに対して、NodeSelectFlameを自動検索して割り当てる
    /// </summary>
    public static void AssignAllNodeFlames()
    {
        var nodes = FindObjectsByType<LaneNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int assigned = 0;

        foreach (var node in nodes)
        {
            var flame = FindSingleDescendantByName(node.transform, NodeFlameName);
            if (flame == null) continue;

            var so = new SerializedObject(node);
            var prop = so.FindProperty("selectFlame");
            prop.objectReferenceValue = flame.gameObject;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(node);
            EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
            assigned++;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Node: {assigned}件 割り当て完了");
    }

    private static void FindAllDescendantsByName(Transform parent, string name, List<GameObject> results)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) results.Add(child.gameObject);
            FindAllDescendantsByName(child, name, results);
        }
    }

    private static Transform FindSingleDescendantByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindSingleDescendantByName(child, name);
            if (result != null) return result;
        }
        return null;
    }
#endif
}

#if UNITY_EDITOR
public static class LaneGlowAutoAssignerMenu
{
    [MenuItem("Tools/Lane/Assign All Segment Flames")]
    private static void AssignSegmentFlames() => LaneGlowAutoAssigner.AssignAllSegmentFlames();

    [MenuItem("Tools/Lane/Assign All Node Flames")]
    private static void AssignNodeFlames() => LaneGlowAutoAssigner.AssignAllNodeFlames();

    [MenuItem("Tools/Lane/Assign All Flames (Segment + Node)")]
    private static void AssignAllFlames()
    {
        LaneGlowAutoAssigner.AssignAllSegmentFlames();
        LaneGlowAutoAssigner.AssignAllNodeFlames();
    }
}
#endif