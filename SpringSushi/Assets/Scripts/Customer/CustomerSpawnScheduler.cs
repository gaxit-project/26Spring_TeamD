using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// spawnIntervalごとの客追加と、ワイプへの客データ投入(EnqueueNextCustomer)を担当する。
/// </summary>
public class CustomerSpawnScheduler : MonoBehaviour
{
    private float spawnInterval;
    private List<CustomerData> customerVariations = new();
    private List<CustomerMoodSO> moodVariations = new();
    private List<ScheduledCustomerEntry> scheduledEntries = new();

    private OrderGenerator orderGenerator;
    private StageProgressTracker progressTracker;
    private bool isRunning;

    /// <summary>
    /// ワイプに客を追加した直後に発火する。
    /// bool引数は「即座に入店を試みてほしいか」の合図(tryAdmit)。
    /// </summary>
    public event System.Action<bool> OnCustomerEnqueued;

    public void Initialize(StageDataSO stageData, OrderGenerator generator, StageProgressTracker tracker)
    {
        spawnInterval = stageData.spawnInterval;
        customerVariations = stageData.customerVariations;
        moodVariations = stageData.moodVariations;
        scheduledEntries = stageData.scheduledEntries;

        orderGenerator = generator;
        progressTracker = tracker;
    }

    public void BeginRunning()
    {
        isRunning = true;
        StartCoroutine(CustomerEntryRoutine());
    }

    public void Stop() => isRunning = false;

    private IEnumerator CustomerEntryRoutine()
    {
        while (isRunning && !progressTracker.IsSpawnComplete)
        {
            yield return new WaitForSeconds(spawnInterval);
            EnqueueNextCustomer();
        }
    }

    public void EnqueueNextCustomer(bool tryAdmit = true)
    {
        if (progressTracker.IsSpawnComplete) return;

        CustomerData data;
        CustomerMoodSO mood;
        int spawnedCount = progressTracker.SpawnedCount;

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

        var waitingData = new WaitingCustomerData
        {
            customerData = data,
            mood = mood,
            orders = orderGenerator.GenerateOrders(data, mood),
        };

        WipeCanvas.Instance?.EnqueueCustomer(waitingData);
        progressTracker.NotifySpawned();

        Debug.Log($"<color=lime>[CustomerSpawnScheduler]</color> ワイプに追加（{progressTracker.SpawnedCount}/{progressTracker.TotalCustomerCount}）");

        OnCustomerEnqueued?.Invoke(tryAdmit);
    }
}