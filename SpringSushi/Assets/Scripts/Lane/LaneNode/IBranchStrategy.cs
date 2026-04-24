using System.Collections.Generic;

public interface IBranchStrategy
{
    LaneSegment Select(List<LaneSegment> options, ref int state);
}