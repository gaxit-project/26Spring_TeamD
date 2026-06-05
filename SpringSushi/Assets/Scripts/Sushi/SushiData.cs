using UnityEngine;

[CreateAssetMenu(fileName = "NewSushiObject", menuName = "Custom/SushiObjectData")]
public class SushiData : ScriptableObject
{
    public string sushiName;
    public int price;
    public GameObject sushiModel;
    public Sprite sushiIcon;

    [Header("生成設定")]
    [Tooltip("この寿司を生成した後の次の生成までのインターバル（秒）")]
    public float spawnInterval = 2.0f;
}