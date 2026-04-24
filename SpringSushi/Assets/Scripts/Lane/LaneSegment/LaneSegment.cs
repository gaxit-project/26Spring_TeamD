using System;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("İ’è")]
    public LaneColor laneColor;

    [HideInInspector] public LaneNode nodeA;
    [HideInInspector] public LaneNode nodeB;

    public bool IsReversed { get; private set; }

    // š ó‘Ô•ÏX’Ê’m
    public event Action<bool> OnReversedChanged;

    public LaneNode GetExitNode() => IsReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => IsReversed ? nodeB : nodeA;

    public string FlowDirectionName =>
        IsReversed ? $"{nodeB.name} ¨ {nodeA.name}" : $"{nodeA.name} ¨ {nodeB.name}";

    public void SetReversed(bool value)
    {
        if (IsReversed == value) return;

        IsReversed = value;

        Debug.Log($"<color=white>yLane Updatez</color> {gameObject.name} ‚Ì—¬‚ê: <b>{FlowDirectionName}</b>");

        // š ‚±‚±‚Å’Ê’m‚¾‚¯
        OnReversedChanged?.Invoke(IsReversed);
    }

    public void Reverse()
    {
        SetReversed(!IsReversed);
    }
}