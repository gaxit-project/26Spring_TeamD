using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ワイプ上で待機中の客1人分のランタイムデータ。
/// </summary>
public class WaitingCustomerData
{
    public CustomerData customerData;
    public CustomerMoodSO mood;
    public List<SushiData> orders; // Mood適用済みの注文リスト
}