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

    [Tooltip("★追加：開店前からワイプに並んでいる客の人数。開店した瞬間（0秒）に全員入店する")]
    [Min(0)]
    public int initialWaitingCustomerCount = 1;

    [Header("客バリエーション（ランダム枠）")]
    public List<CustomerData> customerVariations = new();

    [Header("Mood候補（ランダム枠）")]
    public List<CustomerMoodSO> moodVariations = new();

    [Header("指定来店リスト（順番通りに来店・空欄はランダム）")]
    public List<ScheduledCustomerEntry> scheduledEntries = new();

    [Header("注文候補寿司")]
    public List<SushiData> availableSushiList = new();

    [Header("営業時間設定")]
    [Tooltip("営業時間（秒）")]
    public float operationTime = 60f;
}

[System.Serializable]
public class ScheduledCustomerEntry
{
    [Tooltip("nullならcustomerVariationsからランダム")]
    public CustomerData customerData;
    [Tooltip("nullならmoodVariationsからランダム")]
    public CustomerMoodSO mood;
}