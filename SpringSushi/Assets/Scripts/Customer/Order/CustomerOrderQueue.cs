using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// お客さんの注文キューを管理する。
/// - 全注文数はゲーム開始時に確定（totalOrderCount）
/// - バッチ単位で注文を出す（currentBatch）
/// - バッチが全部届いたら即座に次のバッチを出す
/// - Satisfied = 届いた注文数 / 全注文数
/// </summary>
public class CustomerOrderQueue
{
    // 全注文リスト（ゲーム開始時に確定）
    private readonly List<CustomerOrder> allOrders = new();

    // 現在のバッチ（今注文中のもの）
    private readonly List<CustomerOrder> currentBatch = new();

    // 届いた注文数
    private int deliveredCount = 0;

    // 全注文数
    public int TotalOrderCount => allOrders.Count;

    // 残り注文数
    public int RemainingCount => allOrders.Count - deliveredCount;

    // 現在のバッチ（読み取り専用）
    public IReadOnlyList<CustomerOrder> CurrentBatch => currentBatch;

    // Satisfied（0?1）
    public float SatisfiedRate => TotalOrderCount > 0
        ? (float)deliveredCount / TotalOrderCount
        : 1f;

    // 全注文完了フラグ
    public bool IsAllDelivered => deliveredCount >= TotalOrderCount;

    // 現在のバッチが全部届いたか
    public bool IsBatchComplete => currentBatch.Count > 0 &&
        currentBatch.TrueForAll(o => o.isDelivered);

    // 次のバッチが存在するか
    public bool HasNextBatch => RemainingCount > 0;

    /// <summary>
    /// 全注文を初期化する。
    /// </summary>
    public void Initialize(List<SushiData> orders)
    {
        allOrders.Clear();
        currentBatch.Clear();
        deliveredCount = 0;

        foreach (var data in orders)
            allOrders.Add(new CustomerOrder(data));
    }

    /// <summary>
    /// 次のバッチを取り出す。batchSizeは一度に出す注文数。
    /// </summary>
    public List<CustomerOrder> PullNextBatch(int batchSize)
    {
        currentBatch.Clear();
        int remaining = RemainingCount;
        int count = Mathf.Min(batchSize, remaining);

        // まだ届いていない注文からbatchSize個取り出す
        int pulled = 0;
        foreach (var order in allOrders)
        {
            if (order.isDelivered) continue;
            currentBatch.Add(order);
            pulled++;
            if (pulled >= count) break;
        }

        return currentBatch;
    }

    /// <summary>
    /// 指定したSushiDataの注文を1件届ける。届けられたらtrueを返し、
    /// deliveredOrderにその配達済みCustomerOrderを出力する。
    /// </summary>
    public bool TryDeliver(SushiData sushiData, out CustomerOrder deliveredOrder)
    {
        foreach (var order in currentBatch)
        {
            if (!order.isDelivered && order.sushiData == sushiData)
            {
                order.isDelivered = true;
                deliveredCount++;
                deliveredOrder = order;
                return true;
            }
        }
        deliveredOrder = null;
        return false;
    }
}