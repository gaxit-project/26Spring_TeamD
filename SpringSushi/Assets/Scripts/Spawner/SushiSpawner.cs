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

    // ★ 寿司ごとのクールダウンを管理
    private readonly Dictionary<SushiData, float> cooldowns = new();

    public bool CanManualSpawn => SelectedSushi != null && GetCooldown(SelectedSushi) <= 0f;

    private LaneNode myNode;
    private SushiRegistry registry;

    public void SetMasterNode(LaneNode master) => myNode = master;

    private void Start()
    {
        registry = FindFirstObjectByType<SushiRegistry>();
        if (myNode == null) myNode = GetComponent<LaneNode>();
    }

    private void Update()
    {
        // ★ 全寿司のクールダウンを減らす
        var keys = new List<SushiData>(cooldowns.Keys);
        foreach (var key in keys)
        {
            if (cooldowns[key] > 0f)
                cooldowns[key] -= Time.deltaTime;
        }
    }

    public bool TryManualSpawn()
    {
        if (!CanManualSpawn) return false;

        SushiData sushi = SelectedSushi;
        SpawnSushi(sushi);

        // ★ この寿司のクールダウンだけセット
        cooldowns[sushi] = sushi.spawnInterval;

        return true;
    }

    /// <summary>
    /// 指定した寿司の残りクールダウンを返す。
    /// </summary>
    public float GetCooldown(SushiData sushi)
    {
        if (sushi == null) return 0f;
        return cooldowns.TryGetValue(sushi, out float val) ? Mathf.Max(0f, val) : 0f;
    }

    /// <summary>
    /// 現在選択中の寿司の残りクールダウン（0?1に正規化）。
    /// HUDのクールダウン表示に使う。
    /// </summary>
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