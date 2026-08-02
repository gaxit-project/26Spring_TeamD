using System.Collections.Generic;
using UnityEngine;

/// <summary>イライラ気分。我慢時間が短くなる。旧MoodType.Irritatedに相当する。</summary>
[CreateAssetMenu(fileName = "NewIrritatedMood", menuName = "Custom/CustomerMood/Irritated")]
public class IrritatedMoodSO : CustomerMoodSO
{
    [Header("怒り設定")]
    [Tooltip("PatienceTimeにかける倍率（例：0.5 = 半分の時間で怒る）")]
    [Range(0.1f, 1f)]
    public float patienceMultiplier = 0.5f;

    public override float ModifyPatience(float basePatienceTime) => basePatienceTime * patienceMultiplier;

    public override List<SushiData> GenerateOrders(CustomerData data, List<SushiData> availableSushiList)
    {
        // 注文内容自体は通常と同じ抽選ロジック
        var orders = new List<SushiData>();
        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }
}