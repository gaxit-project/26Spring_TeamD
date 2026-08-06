using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// arrivalWavesに従って客をワイプへ追加する。
/// 1つの波(wave)の客は必ずまとめて生成・追加することで、
/// 同一フレームでの個別生成による重なりを防ぐ。
/// </summary>
public class CustomerSpawnScheduler : MonoBehaviour
{
    private List<CustomerArrivalWave> arrivalWaves = new();
    private List<CustomerData> customerVariations = new();
    private List<CustomerMoodSO> moodVariations = new();
    private List<ScheduledCustomerEntry> scheduledEntries = new();

    private OrderGenerator orderGenerator;
    private StageProgressTracker progressTracker;
    private bool isRunning;

    /// <summary>
    /// 1つの波の客がまとめてワイプに追加された直後に発火する。
    /// 引数はその波の人数(1なら通常の単独客、2以上ならグループ)。
    /// </summary>
    public event System.Action<int> OnWaveEnqueued;

    public void Initialize(StageDataSO stageData, OrderGenerator generator, StageProgressTracker tracker)
    {
        arrivalWaves = stageData.arrivalWaves;
        customerVariations = stageData.customerVariations;
        moodVariations = stageData.moodVariations;
        scheduledEntries = stageData.scheduledEntries;

        orderGenerator = generator;
        progressTracker = tracker;
    }

    public void BeginRunning()
    {
        isRunning = true;
        StartCoroutine(ArrivalRoutine());
    }

    public void Stop() => isRunning = false;

    private IEnumerator ArrivalRoutine()
    {
        foreach (var wave in arrivalWaves)
        {
            if (!isRunning || progressTracker.IsSpawnComplete) yield break;

            if (wave.delay > 0f)
                yield return new WaitForSeconds(wave.delay);

            if (!isRunning || progressTracker.IsSpawnComplete) yield break;

            var batch = BuildCustomerBatch(wave.count);
            if (batch.Count == 0) continue;

            WipeCanvas.Instance?.EnqueueCustomerBatch(batch);
            Debug.Log($"<color=lime>[CustomerSpawnScheduler]</color> ワイプに追加（{progressTracker.SpawnedCount}/{progressTracker.TotalCustomerCount}）");

            OnWaveEnqueued?.Invoke(batch.Count);
        }
    }

    /// <summary>
    /// 開店前の事前並べ(initialWaitingCustomerCount分)用。
    /// waveスケジュールとは独立して、count人分をまとめてワイプへ追加する。
    /// </summary>
    public void PrefillCustomers(int count)
    {
        var batch = BuildCustomerBatch(count);
        if (batch.Count == 0) return;

        WipeCanvas.Instance?.EnqueueCustomerBatch(batch);
        Debug.Log($"<color=lime>[CustomerSpawnScheduler]</color> 事前ワイプ追加（{progressTracker.SpawnedCount}/{progressTracker.TotalCustomerCount}）");
    }

    /// <summary>
    /// count人分の客データを組み立てる(WipeCanvasへはまだ渡さない)。
    /// スケジュール上の来店順(scheduledEntries)を正しく参照するため、
    /// 1人ずつ progressTracker.NotifySpawned() を呼びながら順番に組み立てる。
    /// </summary>
    private List<WaitingCustomerData> BuildCustomerBatch(int count)
    {
        var batch = new List<WaitingCustomerData>();
        for (int i = 0; i < count; i++)
        {
            if (progressTracker.IsSpawnComplete) break;
            batch.Add(BuildCustomerData());
            progressTracker.NotifySpawned();
        }
        return batch;
    }

    private WaitingCustomerData BuildCustomerData()
    {
        int spawnedCount = progressTracker.SpawnedCount;
        CustomerData data;
        CustomerMoodSO mood;

        if (spawnedCount < scheduledEntries.Count)
        {
            var entry = scheduledEntries[spawnedCount];
            data = entry.customerData != null
                ? entry.customerData
                : customerVariations[Random.Range(0, customerVariations.Count)];
            mood = entry.mood != null
                ? entry.mood
                : (moodVariations.Count > 0 ? moodVariations[Random.Range(0, moodVariations.Count)] : null);
        }
        else
        {
            data = customerVariations[Random.Range(0, customerVariations.Count)];
            mood = moodVariations.Count > 0
                ? moodVariations[Random.Range(0, moodVariations.Count)]
                : null;
        }

        return new WaitingCustomerData
        {
            customerData = data,
            mood = mood,
            orders = orderGenerator.GenerateOrders(data, mood),
        };
    }
}