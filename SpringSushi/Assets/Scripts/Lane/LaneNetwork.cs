using System.Collections.Generic;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    [SerializeField] private LaneGraphBuilder graphBuilder;
    [SerializeField] private LaneStateController stateController;
    [SerializeField] private SushiRegistry sushiRegistry;

    private List<LaneSegment> segments;

    private void Awake()
    {
        segments = graphBuilder.Build();
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

        foreach (var seg in segments)
        {
            if (seg.laneColor != color) continue;

            seg.SetReversed(state);

            sushiRegistry.ForEachOnSegment(seg, s =>
            {
                s.SyncDirectionWithSegment();
            });
        }
    }
}