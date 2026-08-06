using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 客データ・気分(Mood)から実際の注文リスト(寿司の組み合わせ)を生成する。
/// 気分ごとの抽選ロジックはCustomerMoodSO側(Strategy)に委譲する。
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
        if (mood != null)
            return mood.GenerateOrders(data, availableSushiList);

        // moodが未設定の場合のデフォルト抽選
        var orders = new List<SushiData>();
        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }
}