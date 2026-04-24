using System.Collections.Generic;
using UnityEngine;

public class RandomBranchStrategy : IBranchStrategy
{
    public LaneSegment Select(List<LaneSegment> options, ref int state)
    {
        return options[Random.Range(0, options.Count)];
    }
}