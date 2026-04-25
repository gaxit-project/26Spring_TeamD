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
        LaneInputManager.OnLaneButtonPressed += OnInput;
    }

    private void OnDisable()
    {
        LaneInputManager.OnLaneButtonPressed -= OnInput;
    }

    private void OnInput(LaneColor color)
    {
        bool state = stateController.Toggle(color);

        // Segment の方向切り替え
        foreach (var seg in segments)
        {
            if (seg.laneColor != color) continue;
            seg.SetReversed(state);
            sushiRegistry.ForEachOnSegment(seg, s => s.SyncDirectionWithSegment());
        }

        // Node の出口切り替え
        foreach (var node in nodes)
        {
            if (node.laneColor != color) continue;
            node.SetReversed(state);
        }

        // 停滞している寿司の再出発を試みる（全員に通知）
        sushiRegistry.TryExitStuckAtNode(null);
    }
}