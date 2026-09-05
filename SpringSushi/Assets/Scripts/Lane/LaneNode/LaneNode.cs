using System;
using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    [Header("レーン設定")]
    public LaneColor laneColor = LaneColor.NoColor;

    [Header("出口設定（ボタンを押すたびに順番に切り替わる）")]
    public List<LaneSegment> exitSegments = new();

    [Header("接続情報（自動収集・参照用）")]
    public List<LaneSegment> connectedSegments = new();

    [Header("選択中エフェクト")]
    [Tooltip("この分岐点が選択中の色に該当するときだけ表示するオブジェクト")]
    [SerializeField] private GameObject selectFlame;

    // 現在選択中の出口インデックス
    private int currentExitIndex = 0;

    public bool IsReversed => currentExitIndex > 0;

    public event Action<int> OnExitIndexChanged;

    public Vector3 Position => transform.position;

    /// <summary>
    /// 現在の出口Segmentを返す
    /// </summary>
    public LaneSegment GetExitSegment()
    {
        if (exitSegments == null || exitSegments.Count == 0) return null;
        return exitSegments[currentExitIndex % exitSegments.Count];
    }

    /// <summary>
    /// SushiMovementから呼ばれる。到着したSegmentを受け取り、次のSegmentを返す。
    /// </summary>
    public LaneSegment GetNextSegment(LaneSegment arrivedFrom)
    {
        var exit = GetExitSegment();
        if (exit == null) return null;
        if (exit == arrivedFrom) return null;
        return exit;
    }

    /// <summary>
    /// ボタン入力時に呼ばれる。出口を次のインデックスへ進める。
    /// </summary>
    public void StepExit()
    {
        if (exitSegments == null || exitSegments.Count == 0) return;
        currentExitIndex = (currentExitIndex + 1) % exitSegments.Count;
        var current = GetExitSegment();
        Debug.Log($"<color=cyan>【Node Update】</color> {gameObject.name} の出口: <b>{(current != null ? current.name : "未設定")}</b> [{currentExitIndex}/{exitSegments.Count}]");
        OnExitIndexChanged?.Invoke(currentExitIndex);
    }

    /// <summary>
    /// 現在の出口インデックスを取得
    /// </summary>
    public int CurrentExitIndex => currentExitIndex;

    /// <summary>
    /// 選択中エフェクト(NodeSelectFlame)の表示/非表示を切り替える。
    /// LaneGlowControllerから呼ばれる。
    /// </summary>
    public void SetGlow(bool visible)
    {
        if (selectFlame != null)
            selectFlame.SetActive(visible);
    }
}