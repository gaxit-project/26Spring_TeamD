using System;
using System.Collections.Generic;
using UnityEngine;

public class SushiRegistry : MonoBehaviour
{
    private readonly List<SushiMovement> list = new();

    public void Register(SushiMovement sushi)
    {
        if (!list.Contains(sushi))
            list.Add(sushi);
    }

    public void Unregister(SushiMovement sushi)
    {
        list.Remove(sushi);
    }

    public void ForEachOnSegment(LaneSegment seg, Action<SushiMovement> action)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var s = list[i];
            if (s == null) { list.RemoveAt(i); continue; }
            if (s.currentSegment == seg) action(s);
        }
    }

    /// <summary>
    /// w’èNode‚Å’â‘Ø‚µ‚Ä‚¢‚éõi‚ÉÄo”­‚ğ‚İ‚³‚¹‚é
    /// </summary>
    public void TryExitStuckAtNode(LaneNode node)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var s = list[i];
            if (s == null) { list.RemoveAt(i); continue; }
            s.TryExitStuck();
        }
    }
}