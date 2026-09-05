using System;
using System.Collections.Generic;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("設定")]
    public LaneColor laneColor;

    [Header("接続ノード")]
    public LaneNode nodeA;
    public LaneNode nodeB;

    [Header("選択中エフェクト（全タイル分）")]
    [Tooltip("この区間に含まれるすべてのSegmentSelectFlame")]
    [SerializeField] private List<GameObject> selectFlames = new();

    public bool IsReversed { get; private set; }
    public event Action<bool> OnReversedChanged;

    public LaneNode GetExitNode() => IsReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => IsReversed ? nodeB : nodeA;

    public string FlowDirectionName =>
        IsReversed ? $"{nodeB.name} → {nodeA.name}" : $"{nodeA.name} → {nodeB.name}";

    private void Awake()
    {
        // インスペクター未割り当て、または動的生成された場合に備えて全タイルから自動収集
        if (selectFlames == null || selectFlames.Count == 0)
        {
            CollectAllFlames();
        }
    }

    /// <summary>
    /// 子孫にあるすべての SegmentSelectFlame を再帰的に収集する
    /// </summary>
    public void CollectAllFlames()
    {
        selectFlames.Clear();
        FindFlamesRecursive(transform, "SegmentSelectFlame", selectFlames);
    }

    private void FindFlamesRecursive(Transform parent, string targetName, List<GameObject> results)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                results.Add(child.gameObject);
            }
            FindFlamesRecursive(child, targetName, results);
        }
    }

    public void SetReversed(bool value)
    {
        if (IsReversed == value) return;
        IsReversed = value;
        Debug.Log($"<color=white>【Lane Update】</color> {gameObject.name} の流れ: <b>{FlowDirectionName}</b>");
        OnReversedChanged?.Invoke(IsReversed);
    }

    public void Reverse()
    {
        SetReversed(!IsReversed);
    }

    /// <summary>
    /// この区間に含まれる【すべてのタイル】のSegmentSelectFlameの表示/非表示を切り替える
    /// </summary>
    public void SetGlow(bool visible)
    {
        if (selectFlames == null || selectFlames.Count == 0)
        {
            CollectAllFlames();
        }

        for (int i = 0; i < selectFlames.Count; i++)
        {
            if (selectFlames[i] != null)
            {
                selectFlames[i].SetActive(visible);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor拡張用：リストを外部から一括設定
    /// </summary>
    public void EditorSetSelectFlames(List<GameObject> flames)
    {
        selectFlames = flames;
    }
#endif
}