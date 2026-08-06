using System;

/// <summary>
/// ステージ全体の来店数・退店数を管理し、クリア判定を行う。
/// </summary>
public class StageProgressTracker
{
    public int TotalCustomerCount { get; private set; }
    public int SpawnedCount { get; private set; }
    public int ExitedCount { get; private set; }

    public bool IsSpawnComplete => SpawnedCount >= TotalCustomerCount;
    public bool IsStageComplete => SpawnedCount >= TotalCustomerCount && ExitedCount >= TotalCustomerCount;

    /// <summary>全客退店(=ステージクリア)を通知する。</summary>
    public event Action OnStageClear;

    public void Initialize(int totalCustomerCount)
    {
        TotalCustomerCount = totalCustomerCount;
        SpawnedCount = 0;
        ExitedCount = 0;
    }

    public void NotifySpawned() => SpawnedCount++;

    public void NotifyExited()
    {
        ExitedCount++;
        if (IsStageComplete)
            OnStageClear?.Invoke();
    }
}