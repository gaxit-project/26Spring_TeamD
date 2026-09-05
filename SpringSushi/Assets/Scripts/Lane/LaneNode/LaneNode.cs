using System;
using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    [Header("レーン設定")]
    public LaneColor laneColor = LaneColor.NoColor;

    [Header("出口設定")]
    public List<LaneSegment> exitSegments = new();

    [Header("接続情報")]
    public List<LaneSegment> connectedSegments = new();

    [Header("選択中エフェクト（全Flame分）")]
    [Tooltip("この分岐点に含まれるすべてのNodeSelectFlame")]
    [SerializeField] private List<GameObject> selectFlames = new();

    [Header("決定時エフェクト（全Flame分）")]
    [Tooltip("この分岐点に含まれるすべてのRedNodeSelectFlame")]
    [SerializeField] private List<GameObject> redSelectFlames = new();

    // 毎フレームのGetComponent負荷を避けるためのキャッシュ
    private List<SpriteRenderer> flameRenderers = new();

    private int currentExitIndex = 0;

    public bool IsReversed => currentExitIndex > 0;

    public event Action<int> OnExitIndexChanged;

    public Vector3 Position => transform.position;

    private void Awake()
    {
        if (selectFlames == null || selectFlames.Count == 0)
        {
            CollectAllFlames();
        }
        else
        {
            CacheRenderers();
        }
    }

    /// <summary>
    /// このノード配下のすべてのNodeSelectFlame/RedNodeSelectFlameを再帰的に収集する。
    /// </summary>
    public void CollectAllFlames()
    {
        selectFlames.Clear();
        redSelectFlames.Clear();
        FindFlamesRecursive(transform, "NodeSelectFlame", selectFlames);
        FindFlamesRecursive(transform, "RedNodeSelectFlame", redSelectFlames);
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

    public LaneSegment GetExitSegment()
    {
        if (exitSegments == null || exitSegments.Count == 0) return null;
        return exitSegments[currentExitIndex % exitSegments.Count];
    }

    public LaneSegment GetNextSegment(LaneSegment arrivedFrom)
    {
        var exit = GetExitSegment();
        if (exit == null || exit == arrivedFrom) return null;
        return exit;
    }

    public void StepExit()
    {
        if (exitSegments == null || exitSegments.Count == 0) return;
        currentExitIndex = (currentExitIndex + 1) % exitSegments.Count;
        OnExitIndexChanged?.Invoke(currentExitIndex);
    }

    public int CurrentExitIndex => currentExitIndex;

    /// <summary>
    /// 選択中エフェクト(NodeSelectFlame)の表示/非表示を切り替える。
    /// このノードに含まれる全タイル分のFlameに一括反映する。
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
    /// 決定時の一瞬の強調表示(RedNodeSelectFlame)の表示/非表示を切り替える。
    /// このノードに含まれる全タイル分のFlameに一括反映する。
    /// </summary>
    public void SetRedGlow(bool visible)
    {
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
            else if (i < selectFlames.Count && selectFlames[i] != null)
            {
                selectFlames[i].SetActive(isHardVisible);
            }
        }
    }

#if UNITY_EDITOR
    public void EditorSetFlames(List<GameObject> normalFlames, List<GameObject> redFlames)
    {
        selectFlames = normalFlames;
        redSelectFlames = redFlames;
        CacheRenderers();
    }
#endif
}