using System.Collections.Generic;
using UnityEngine;

/// <summary>腹ペコ気分。注文数が増える。旧MoodType.Hungryに相当する。</summary>
[CreateAssetMenu(fileName = "NewHungryMood", menuName = "Custom/CustomerMood/Hungry")]
public class HungryMoodSO : CustomerMoodSO
{
    [Header("腹ペコ設定")]
    [Tooltip("通常の注文数にこの倍率をかける（例：2 = 2倍注文）")]
    public float orderCountMultiplier = 2f;

    public override float ModifyPatience(float basePatienceTime) => basePatienceTime;

    public override List<SushiData> GenerateOrders(CustomerData data, List<SushiData> availableSushiList)
    {
        var orders = new List<SushiData>();
        int hungryCount = Mathf.RoundToInt(
            Random.Range(1, data.maxTotalOrders + 1) * orderCountMultiplier);
        hungryCount = Mathf.Max(1, hungryCount);
        for (int i = 0; i < hungryCount; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }
}