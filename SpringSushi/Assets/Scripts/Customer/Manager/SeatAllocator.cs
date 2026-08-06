using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ÀÈ‚Ì—\–ñE‰ğ•ú‚ğŠÇ—‚·‚éBÀÈ‹£‡‚ğ–h‚®‚½‚ß‚Ì—Bˆê‚Ì‘‹ŒûB
/// </summary>
public class SeatAllocator
{
    private readonly List<Transform> entryPoints;
    private readonly HashSet<Transform> reservedSeats = new();

    public SeatAllocator(List<Transform> entryPoints)
    {
        this.entryPoints = entryPoints;
    }

    public void Clear() => reservedSeats.Clear();

    /// <summary>
    /// origin‚ÉÅ‚à‹ß‚¢A‹ó‚¢‚Ä‚¢‚éÀÈ‚ğ1‚Â’T‚µAŒ©‚Â‚©‚Á‚½‚»‚Ìê‚Å—\–ñ‚Ü‚Ås‚¤
    /// (æ“¾‚Æ—\–ñ‚ğ•ª—£‚µ‚È‚¢)BŒ©‚Â‚©‚ç‚È‚¯‚ê‚Înull‚ğ•Ô‚·B
    /// </summary>
    public Transform TryReserveSeat(Vector3 origin)
    {
        Transform nearest = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (var seat in entryPoints)
        {
            if (seat == null) continue;
            if (reservedSeats.Contains(seat)) continue;

            float distanceSqr = (seat.position - origin).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearest = seat;
            }
        }

        if (nearest != null)
            reservedSeats.Add(nearest);

        return nearest;
    }

    public void ReleaseSeat(Transform seat)
    {
        reservedSeats.Remove(seat);
    }
}