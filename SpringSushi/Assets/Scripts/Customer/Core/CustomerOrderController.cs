using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注文の受付・配達・バッチ管理を担当する。
/// </summary>
public class CustomerOrderController : MonoBehaviour
{
    private CustomerOrderQueue orderQueue = new();
    public CustomerOrderQueue OrderQueue => orderQueue;
    public int BatchCount { get; private set; } = 0;

    public void Initialize(List<SushiData> orders)
    {
        BatchCount = 0;
        orderQueue.Initialize(orders);
    }

    public void PullNextBatch(int batchSize)
    {
        BatchCount++;
        orderQueue.PullNextBatch(batchSize);
    }

    public bool TryDeliver(SushiData sushiData)
    {
        return orderQueue.TryDeliver(sushiData);
    }
}