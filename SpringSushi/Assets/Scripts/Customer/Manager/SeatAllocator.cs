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
    /// ‹ó‚¢‚Ä‚¢‚éÀÈ‚ğ1‚Â’T‚µAŒ©‚Â‚©‚Á‚½‚»‚Ìê‚Å—\–ñ‚Ü‚Ås‚¤(æ“¾‚Æ—\–ñ‚ğ•ª—£‚µ‚È‚¢)B
    /// Œ©‚Â‚©‚ç‚È‚¯‚ê‚Înull‚ğ•Ô‚·B
    /// </summary>
    public Transform TryReserveSeat()
    {
        foreach (var seat in entryPoints)
        {
            if (reservedSeats.Contains(seat)) continue;
            reservedSeats.Add(seat);
            return seat;
        }
        return null;
    }

    public void ReleaseSeat(Transform seat)
    {
        reservedSeats.Remove(seat);
    }
}