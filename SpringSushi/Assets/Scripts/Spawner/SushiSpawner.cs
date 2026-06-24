using UnityEngine;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("Prefab・データ")]
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;

    public int SelectedIndex { get; private set; } = 0;
    public SushiData SelectedSushi =>
        (sushiDataList != null && sushiDataList.Count > 0)
            ? sushiDataList[SelectedIndex] : null;

    private readonly Dictionary<SushiData, float> cooldowns = new();

    private LaneNode myNode;
    private SushiRegistry registry;

    // ★ 追加：営業開始（スタート操作）まで生成を止めるフラグ
    public bool IsSpawningEnabled { get; private set; } = false;

    public void SetMasterNode(LaneNode master) => myNode = master;

    // ★ 追加：InGameSequenceManagerなど外部から呼ぶ
    public void SetSpawningEnabled(bool enabled)
    {
        IsSpawningEnabled = enabled;

        // ★ 生成開始時にクールダウンを全寿司リセットしておく
        //    （開店前に裏で進んでいたタイマーが残らないようにする）
        if (enabled && sushiDataList != null)
        {
            foreach (var sushi in sushiDataList)
            {
                if (sushi == null) continue;
                cooldowns[sushi] = 0f; // 開店直後は即生成可能にする
            }
        }
    }

    private void Start()
    {
        registry = FindFirstObjectByType<SushiRegistry>();
        if (myNode == null) myNode = GetComponent<LaneNode>();
    }

    private void Update()
    {
        // ★ 生成許可が出るまでは何もしない
        if (!IsSpawningEnabled) return;
        if (sushiDataList == null) return;

        foreach (var sushi in sushiDataList)
        {
            if (sushi == null) continue;

            if (!cooldowns.ContainsKey(sushi))
                cooldowns[sushi] = 0f;

            if (cooldowns[sushi] > 0f)
                cooldowns[sushi] -= Time.deltaTime;
        }

        SushiData selected = SelectedSushi;
        if (selected != null && GetCooldown(selected) <= 0f)
        {
            SpawnSushi(selected);
            cooldowns[selected] = selected.spawnInterval;
        }
    }

    /*
    public bool CanManualSpawn => SelectedSushi != null && GetCooldown(SelectedSushi) <= 0f;

    public bool TryManualSpawn()
    {
        if (!CanManualSpawn) return false;
        SushiData sushi = SelectedSushi;
        SpawnSushi(sushi);
        cooldowns[sushi] = sushi.spawnInterval;
        return true;
    }
    */

    public float GetCooldown(SushiData sushi)
    {
        if (sushi == null) return 0f;
        return cooldowns.TryGetValue(sushi, out float val) ? Mathf.Max(0f, val) : 0f;
    }

    public float GetCooldownRate()
    {
        var sushi = SelectedSushi;
        if (sushi == null || sushi.spawnInterval <= 0f) return 0f;
        return GetCooldown(sushi) / sushi.spawnInterval;
    }

    public void ShiftSelection(int direction)
    {
        if (sushiDataList == null || sushiDataList.Count == 0) return;
        SelectedIndex = (SelectedIndex + direction + sushiDataList.Count) % sushiDataList.Count;
    }

    private void SpawnSushi(SushiData data)
    {
        if (sushiBasePrefab == null || data == null || myNode == null) return;
        if (myNode.exitSegments == null || myNode.exitSegments.Count == 0)
        {
            Debug.LogWarning($"[SushiSpawner] {myNode.name} のexitSegmentsが未設定です。");
            return;
        }
        LaneSegment targetSegment = myNode.exitSegments[0];
        if (targetSegment == null) return;

        GameObject obj = Instantiate(sushiBasePrefab, myNode.Position, Quaternion.identity);
        SushiMovement move = obj.GetComponent<SushiMovement>();
        move.Initialize(data, targetSegment, myNode, registry);

        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.PlaySFX(SoundKeys.SushiSpawn);
    }

    public Vector3 WorldPosition => myNode != null ? myNode.Position : transform.position;
}