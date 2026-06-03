using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Custom/StageData")]
public class StageDataSO : ScriptableObject
{
    [Header("ステージ情報")]
    public string stageName;

    [Header("客スポーン設定")]
    [Tooltip("このステージで来店する総客数")]
    public int totalCustomerCount = 10;
    [Tooltip("客のスポーン間隔（秒）")]
    public float spawnInterval = 5f;

    [Header("客バリエーション")]
    public List<CustomerData> customerVariations = new();

    [Header("注文候補寿司")]
    public List<SushiData> availableSushiList = new();
}