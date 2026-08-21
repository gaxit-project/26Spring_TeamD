using System.Collections.Generic;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    [SerializeField] private LaneGraphBuilder graphBuilder;
    [SerializeField] private LaneStateController stateController;
    [SerializeField] private SushiRegistry sushiRegistry;

    private List<LaneSegment> segments;
    private List<LaneNode> nodes;

    private void Awake()
    {
        (segments, nodes) = graphBuilder.Build();
    }

    private void OnEnable()
    {
        LaneInputManager.OnLaneButtonPressed += ToggleLane;
    }

    private void OnDisable()
    {
        LaneInputManager.OnLaneButtonPressed -= ToggleLane;
    }

    /// <summary>
    /// 指定した色のレーンの進行方向を反転させる。
    /// ダイレクト方式(LaneInputManagerのボタン)・選択方式(LaneSelectorUI)の
    /// どちらからも、この公開メソッドを直接呼び出す。
    /// </summary>
    public void ToggleLane(LaneColor color)
    {
        Debug.Log($"[LaneNetwork] ToggleLane実行! 色={color}, 自分のInstanceID={GetInstanceID()}, セグメント数={segments?.Count ?? 0}");

        bool state = stateController.Toggle(color);
        Debug.Log($"[LaneNetwork] StateController反転結果: state = {state}");

        int updatedCount = 0;
        foreach (var seg in segments)
        {
            if (seg.laneColor != color) continue;

            updatedCount++; // ← 該当する色のセグメントが見つかったらカウント

            seg.SetReversed(state);
            sushiRegistry.ForEachOnSegment(seg, s => s.SyncDirectionWithSegment());
        }

        // ★ここに注目：実際に何個のセグメントの色が一致して反転したかを出力
        Debug.Log($"[LaneNetwork] 実際に反転処理されたセグメント数 (matched): {updatedCount}");

        foreach (var node in nodes)
        {
            if (node.laneColor != color) continue;
            node.StepExit();
        }

        sushiRegistry.TryExitStuckAtNode(null);
    }

    /// <summary>
    /// このステージ(シーン)に、指定した色のセグメントが実際に1つでも存在するかを返す。
    /// LaneSelectorUIが、存在しない色のボタンを選択候補から除外するために使う。
    /// </summary>
    public bool HasLane(LaneColor color)
    {
        foreach (var seg in segments)
        {
            if (seg.laneColor == color) return true;
        }
        return false;
    }
}