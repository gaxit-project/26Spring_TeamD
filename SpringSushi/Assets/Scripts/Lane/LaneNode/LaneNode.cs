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

    [Header("選択中エフェクト")]
    [SerializeField] private GameObject selectFlame;
    [SerializeField] private GameObject redSelectFlame; // 追加: 赤色Flame

    private SpriteRenderer flameRenderer;
    private int currentExitIndex = 0;

    public bool IsReversed => currentExitIndex > 0;
    public event Action<int> OnExitIndexChanged;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        CacheRenderer();
        if (redSelectFlame == null && selectFlame != null && selectFlame.transform.parent != null)
        {
            var redTransform = selectFlame.transform.parent.Find("RedNodeSelectFlame");
            if (redTransform != null) redSelectFlame = redTransform.gameObject;
        }
    }

    private void CacheRenderer()
    {
        if (selectFlame != null && flameRenderer == null)
            flameRenderer = selectFlame.GetComponent<SpriteRenderer>();
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

    public void SetGlow(bool visible)
    {
        if (selectFlame != null)
        {
            selectFlame.SetActive(visible);
            CacheRenderer();
        }
    }

    /// <summary>
    /// 赤色Flame（決定時の一瞬の強調）の表示・非表示
    /// </summary>
    public void SetRedGlow(bool visible)
    {
        if (redSelectFlame != null)
        {
            redSelectFlame.SetActive(visible);
        }
    }

    public void UpdateBlink(float alpha, bool isHardVisible)
    {
        if (flameRenderer != null)
        {
            Color c = flameRenderer.color;
            c.a = alpha;
            flameRenderer.color = c;
        }
        else if (selectFlame != null)
        {
            selectFlame.SetActive(isHardVisible);
        }
    }

#if UNITY_EDITOR
    public void EditorSetFlames(GameObject normalFlame, GameObject redFlame)
    {
        selectFlame = normalFlame;
        redSelectFlame = redFlame;
    }
#endif
}