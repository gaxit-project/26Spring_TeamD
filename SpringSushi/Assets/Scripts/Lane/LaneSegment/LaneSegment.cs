using System;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("ê›íË")]
    public LaneColor laneColor;

    [Header("ê⁄ë±ÉmÅ[Éh")]
    public LaneNode nodeA;
    public LaneNode nodeB;

    public bool IsReversed { get; private set; }

    public event Action<bool> OnReversedChanged;

    public LaneNode GetExitNode() => IsReversed ? nodeA : nodeB;
    public LaneNode GetEntryNode() => IsReversed ? nodeB : nodeA;

    public string FlowDirectionName =>
        IsReversed ? $"{nodeB.name} Å® {nodeA.name}" : $"{nodeA.name} Å® {nodeB.name}";

    public void SetReversed(bool value)
    {
        if (IsReversed == value) return;
        IsReversed = value;
        Debug.Log($"<color=white>ÅyLane UpdateÅz</color> {gameObject.name} ÇÃó¨ÇÍ: <b>{FlowDirectionName}</b>");
        OnReversedChanged?.Invoke(IsReversed);
    }

    public void Reverse()
    {
        SetReversed(!IsReversed);
    }
}