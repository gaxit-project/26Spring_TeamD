using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    public List<LaneSegment> connectedSegments = new();

    public bool isBranch = false;

    private int strategyState = 0;
    private IBranchStrategy branchStrategy;

    // š ‚±‚ê‚ð•K‚¸’Ç‰Á
    public Vector3 Position => transform.position;

    private void Awake()
    {
        branchStrategy = new AlternatingBranchStrategy();
    }

    public void SetStrategy(IBranchStrategy strategy)
    {
        branchStrategy = strategy;
    }

    public LaneSegment GetNextSegment(LaneSegment current)
    {
        List<LaneSegment> options = new();

        foreach (var seg in connectedSegments)
        {
            if (seg == current) continue;

            if (IsEnterable(seg, this))
            {
                options.Add(seg);
            }
        }

        if (options.Count == 0) return null;

        if (!isBranch || options.Count == 1)
        {
            return options[0];
        }

        return branchStrategy.Select(options, ref strategyState);
    }

    private bool IsEnterable(LaneSegment seg, LaneNode fromNode)
    {
        if (!seg.IsReversed)
            return fromNode == seg.nodeA;
        else
            return fromNode == seg.nodeB;
    }
}