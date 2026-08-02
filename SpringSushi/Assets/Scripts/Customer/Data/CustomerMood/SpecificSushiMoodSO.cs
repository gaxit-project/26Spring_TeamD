using System.Collections.Generic;
using UnityEngine;

/// <summary>特定の寿司だけを注文する気分。旧MoodType.SpecificSushiに相当する。</summary>
[CreateAssetMenu(fileName = "NewSpecificSushiMood", menuName = "Custom/CustomerMood/SpecificSushi")]
public class SpecificSushiMoodSO : CustomerMoodSO
{
    [Header("特定寿司注文")]
    [Tooltip("必ず注文する寿司リスト")]
    public List<SushiData> fixedOrders = new();

    public override float ModifyPatience(float basePatienceTime) => basePatienceTime;

    public override List<SushiData> GenerateOrders(CustomerData data, List<SushiData> availableSushiList)
    {
        var orders = new List<SushiData>();
        if (fixedOrders.Count == 0) return orders;

        int specificCount = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < specificCount; i++)
        {
            int idx = Random.Range(0, fixedOrders.Count);
            orders.Add(fixedOrders[idx]);
        }
        return orders;
    }
}