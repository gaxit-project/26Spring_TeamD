using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerData", menuName = "Custom/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerType;    // 「子供」「大食い」「グルメ」など
    public GameObject customerPrefab; // お客さんの見た目
    public float patienceTime;     // 待ち時間（怒るまでの速さ）
    public float eatTime;          // 食べるスピード
    public int scoreMultiplier;    // 満足した時にもらえるスコア倍率
}