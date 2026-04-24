using System.Collections.Generic;
using UnityEngine;

public class LaneStateController : MonoBehaviour
{
    private Dictionary<LaneColor, bool> states = new();

    public bool Toggle(LaneColor color)
    {
        if (!states.ContainsKey(color))
            states[color] = false;

        states[color] = !states[color];
        return states[color];
    }

    public bool GetState(LaneColor color)
    {
        return states.TryGetValue(color, out var v) && v;
    }
}