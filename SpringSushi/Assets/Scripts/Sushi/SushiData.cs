using UnityEngine;

[CreateAssetMenu(fileName = "NewSushiData", menuName = "Sushi/SushiData")]
public class SushiData : ScriptableObject
{
    public string sushiName;    // 名前（まぐろ、たまご等）
    public int price;           // 値段
    public GameObject modelPrefab; // その寿司に適した3Dモデル
}