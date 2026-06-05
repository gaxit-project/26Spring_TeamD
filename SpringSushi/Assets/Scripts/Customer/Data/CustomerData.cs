using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "Custom/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerType;
    public GameObject customerPrefab;

    [Header("注文設定")]
    [Tooltip("何注文以内で必ず帰るか")]
    public int maxOrderBatches = 3;
    [Tooltip("全注文数の最大値")]
    public int maxTotalOrders = 5;
    [Tooltip("一度に注文する個数の最小値")]
    public int batchSizeMin = 1;
    [Tooltip("一度に注文する個数の最大値")]
    public int batchSizeMax = 2;
    [Tooltip("1バッチを食べ終えた後の待機時間（秒）")]
    public float eatTime = 4f;

    [Header("Patience設定")]
    public float basePatienceTime = 10f;
    [Range(0.5f, 1f)]
    public float patienceDecayRate = 0.9f;

    [Header("Angry設定")]
    [Tooltip("怒ってから帰るまでの時間（秒）")]
    public float angryTime = 3f;

    [Header("スコア")]
    public float scoreMultiplier = 1f;
}