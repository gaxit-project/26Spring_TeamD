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
    private const string RedSegmentFlameName = "RedSegmentSelectFlame";
    private const string NodeFlameName = "NodeSelectFlame";
    private const string RedNodeFlameName = "RedNodeSelectFlame";

    public static void AssignAllSegmentFlames()
    {
        var segments = FindObjectsByType<LaneSegment>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int totalFlameCount = 0;
        int totalRedCount = 0;

        foreach (var seg in segments)
        {
            var flames = new List<GameObject>();
            var redFlames = new List<GameObject>();

            FindAllDescendantsByName(seg.transform, SegmentFlameName, flames);
            FindAllDescendantsByName(seg.transform, RedSegmentFlameName, redFlames);

            seg.EditorSetSelectFlames(flames, redFlames);
            EditorUtility.SetDirty(seg);
            EditorSceneManager.MarkSceneDirty(seg.gameObject.scene);

            totalFlameCount += flames.Count;
            totalRedCount += redFlames.Count;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Segment: {segments.Length}ãÊä‘ (í èÌ: {totalFlameCount}å¬, ê‘: {totalRedCount}å¬) äÑÇËìñÇƒäÆóπ");
    }

    public static void AssignAllNodeFlames()
    {
        var nodes = FindObjectsByType<LaneNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int assigned = 0;

        foreach (var node in nodes)
        {
            var flame = FindSingleDescendantByName(node.transform, NodeFlameName);
            var redFlame = FindSingleDescendantByName(node.transform, RedNodeFlameName);

            node.EditorSetFlames(flame != null ? flame.gameObject : null, redFlame != null ? redFlame.gameObject : null);

            EditorUtility.SetDirty(node);
            EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
            assigned++;
        }

        Debug.Log($"<color=green>[LaneGlowAutoAssigner]</color> Node: {assigned}åè äÑÇËìñÇƒäÆóπ");
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