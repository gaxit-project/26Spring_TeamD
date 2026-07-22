using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 客データ・気分(Mood)から実際の注文リスト(寿司の組み合わせ)を生成する。
/// </summary>
public class OrderGenerator
{
    private List<SushiData> availableSushiList;

    public void Initialize(List<SushiData> stageAvailableSushiList)
    {
        availableSushiList = stageAvailableSushiList;
    }

    public List<SushiData> GenerateOrders(CustomerData data, CustomerMoodSO mood)
    {
        var orders = new List<SushiData>();

        if (mood != null)
        {
            switch (mood.moodType)
            {
                case CustomerMoodSO.MoodType.SpecificSushi:
                    if (mood.fixedOrders.Count > 0)
                    {
                        int specificCount = Random.Range(1, data.maxTotalOrders + 1);
                        for (int i = 0; i < specificCount; i++)
                        {
                            int idx = Random.Range(0, mood.fixedOrders.Count);
                            orders.Add(mood.fixedOrders[idx]);
                        }
                    }
                    return orders;

                case CustomerMoodSO.MoodType.Hungry:
                    int hungryCount = Mathf.RoundToInt(
                        Random.Range(1, data.maxTotalOrders + 1) * mood.orderCountMultiplier);
                    hungryCount = Mathf.Max(1, hungryCount);
                    for (int i = 0; i < hungryCount; i++)
                        orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
                    return orders;
            }
        }

        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }
}