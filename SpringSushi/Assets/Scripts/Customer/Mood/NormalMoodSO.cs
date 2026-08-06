using System.Collections.Generic;
using UnityEngine;

/// <summary>通常の気分。特別な補正なし。旧MoodType.Randomに相当する。</summary>
[CreateAssetMenu(fileName = "NewNormalMood", menuName = "Custom/CustomerMood/Normal")]
public class NormalMoodSO : CustomerMoodSO
{
    public override bool ShowIndicator => false;

    public override float ModifyPatience(float basePatienceTime) => basePatienceTime;

    public override List<SushiData> GenerateOrders(CustomerData data, List<SushiData> availableSushiList)
    {
        var orders = new List<SushiData>();
        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }
}