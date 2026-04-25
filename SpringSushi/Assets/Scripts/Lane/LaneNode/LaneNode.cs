using System;
using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    [Header("レーン設定")]
    public LaneColor laneColor = LaneColor.NoColor;

    [Header("出口設定")]
    public LaneSegment defaultExit;   // IsReversed=false のとき
    public LaneSegment reversedExit;  // IsReversed=true のとき

    [Header("接続情報（自動収集・参照用）")]
    public List<LaneSegment> connectedSegments = new();

    public bool IsReversed { get; private set; }

    public event Action<bool> OnReversedChanged;

    public Vector3 Position => transform.position;

    /// <summary>
    /// 現在のIsReversed状態に基づいて出口Segmentを返す
    /// </summary>
    public LaneSegment GetExitSegment()
    {
        return IsReversed ? reversedExit : defaultExit;
    }

    /// <summary>
    /// SushiMovementから呼ばれる。到着したSegmentを受け取り、次のSegmentを返す。
    /// </summary>
    public LaneSegment GetNextSegment(LaneSegment arrivedFrom)
    {
        var exit = GetExitSegment();
        if (exit == null) return null;
        // 到着したSegmentと同じなら進めない（折り返し防止）
        if (exit == arrivedFrom) return null;
        return exit;
    }

    public void SetReversed(bool value)
    {
        if (IsReversed == value) return;
        IsReversed = value;
        Debug.Log($"<color=cyan>【Node Update】</color> {gameObject.name} の出口: <b>{(IsReversed ? (reversedExit != null ? reversedExit.name : "未設定") : (defaultExit != null ? defaultExit.name : "未設定"))}</b>");
        OnReversedChanged?.Invoke(IsReversed);
    }
}