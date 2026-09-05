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

    [Tooltip("この区間に含まれるすべてのRedSegmentSelectFlame")]
    [SerializeField] private List<GameObject> redSelectFlames = new();

    // 毎フレームのGetComponent負荷を避けるためのキャッシュ
    private List<SpriteRenderer> flameRenderers = new();

    public bool IsReversed { get; private set; }
    public event Action<bool> OnReversedChanged;

    public LaneNode GetExitNode() => IsReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => IsReversed ? nodeB : nodeA;

    public string FlowDirectionName =>
        IsReversed ? $"{nodeB.name} → {nodeA.name}" : $"{nodeA.name} → {nodeB.name}";

    private void Awake()
    {
        if (selectFlames == null || selectFlames.Count == 0 || redSelectFlames == null || redSelectFlames.Count == 0)
        {
            CollectAllFlames();
        }
        else
        {
            CacheRenderers();
        }
    }

    public void CollectAllFlames()
    {
        selectFlames.Clear();
        redSelectFlames.Clear();
        FindFlamesRecursive(transform, "SegmentSelectFlame", selectFlames);
        FindFlamesRecursive(transform, "RedSegmentSelectFlame", redSelectFlames);
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        flameRenderers.Clear();
        for (int i = 0; i < selectFlames.Count; i++)
        {
            if (selectFlames[i] != null)
                flameRenderers.Add(selectFlames[i].GetComponent<SpriteRenderer>());
            else
                flameRenderers.Add(null);
        }
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

    public void Reverse() => SetReversed(!IsReversed);

    /// <summary>
    /// 表示/非表示の切り替え（非選択色を完全にOFFにする）
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
                selectFlames[i].SetActive(visible);
        }
    }

    /// <summary>
    /// 赤色Flame（決定時の一瞬の強調）の表示/非表示
    /// </summary>
    public void SetRedGlow(bool visible)
    {
        if (redSelectFlames == null || redSelectFlames.Count == 0)
        {
            CollectAllFlames();
        }

        for (int i = 0; i < redSelectFlames.Count; i++)
        {
            if (redSelectFlames[i] != null)
                redSelectFlames[i].SetActive(visible);
        }
    }

    /// <summary>
    /// 選択中のレーン専用：点滅のアルファ値（透明度）を更新
    /// </summary>
    public void UpdateBlink(float alpha, bool isHardVisible)
    {
        for (int i = 0; i < flameRenderers.Count; i++)
        {
            var sr = flameRenderers[i];
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            else if (selectFlames[i] != null)
            {
                selectFlames[i].SetActive(isHardVisible);
            }
        }
    }

#if UNITY_EDITOR
    public void EditorSetSelectFlames(List<GameObject> flames, List<GameObject> redFlames)
    {
        selectFlames = flames;
        redSelectFlames = redFlames;
        CacheRenderers();
    }
#endif
}