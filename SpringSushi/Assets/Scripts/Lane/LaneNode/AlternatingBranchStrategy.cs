using System.Collections.Generic;

public class AlternatingBranchStrategy : IBranchStrategy
{
    public LaneSegment Select(List<LaneSegment> options, ref int state)
    {
        var selected = options[state % options.Count];
        state++;
        return selected;
    }
}