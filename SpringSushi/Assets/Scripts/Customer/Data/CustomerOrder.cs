/// <summary>
/// 注文1件分のデータ。
/// </summary>
[System.Serializable]
public class CustomerOrder
{
    public SushiData sushiData;
    public bool isDelivered = false;

    public CustomerOrder(SushiData data)
    {
        sushiData = data;
        isDelivered = false;
    }
}