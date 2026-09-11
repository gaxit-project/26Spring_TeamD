using UnityEngine;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("Prefab・データ")]
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;

    [Header("切り替え後の生成ディレイ（秒）")]
    [SerializeField] private float selectionSwitchDelay = 0.5f;

    [Header("フィーバー設定")]
    [Tooltip("フィーバー中の寿司生成インターバル（秒）")]
    [SerializeField] private float feverSpawnInterval = 1.0f; // ★ フィーバー時は1秒

    public int SelectedIndex { get; private set; } = 0;
    public SushiData SelectedSushi =>
        (sushiDataList != null && sushiDataList.Count > 0)
            ? sushiDataList[SelectedIndex] : null;

    private readonly Dictionary<SushiData, float> cooldowns = new();

    private float selectionSwitchTimer = 0f;

    private LaneNode myNode;
    private SushiRegistry registry;

    public bool IsSpawningEnabled { get; private set; } = false;

    public void SetMasterNode(LaneNode master) => myNode = master;

    public void SetSpawningEnabled(bool enabled)
    {
        IsSpawningEnabled = enabled;

        if (enabled && sushiDataList != null)
        {
            foreach (var sushi in sushiDataList)
            {
                if (sushi == null) continue;
                cooldowns[sushi] = 0f;
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
        if (!IsSpawningEnabled) return;
        if (sushiDataList == null) return;

        if (selectionSwitchTimer > 0f)
        {
            selectionSwitchTimer -= Time.deltaTime;
            return;
        }

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

            // ★ フィーバー中は1秒、通常時は各寿司データの秒数をセット
            cooldowns[selected] = GetEffectiveSpawnInterval(selected);
        }
    }

    /// <summary>
    /// 現在の状況（通常 / フィーバー）に応じた生成インターバルを取得
    /// </summary>
    private float GetEffectiveSpawnInterval(SushiData data)
    {
        if (data == null) return 0f;

        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;
        return isFever ? feverSpawnInterval : data.spawnInterval;
    }

    public void ShiftSelection(int direction)
    {
        if (sushiDataList == null || sushiDataList.Count == 0) return;
        SelectedIndex = (SelectedIndex + direction + sushiDataList.Count) % sushiDataList.Count;

        selectionSwitchTimer = selectionSwitchDelay;

        var newlySelected = SelectedSushi;
        if (newlySelected != null)
        {
            float currentCooldown = GetCooldown(newlySelected);
            cooldowns[newlySelected] = Mathf.Max(currentCooldown, selectionSwitchDelay);
        }
    }

    public float GetCooldown(SushiData sushi)
    {
        if (sushi == null) return 0f;
        return cooldowns.TryGetValue(sushi, out float val) ? Mathf.Max(0f, val) : 0f;
    }

    public float GetCooldownRate()
    {
        var sushi = SelectedSushi;
        if (sushi == null) return 0f;

        // ★ UIの円ゲージ等の割合計算も、現在の有効なインターバル基準にする
        float interval = GetEffectiveSpawnInterval(sushi);
        if (interval <= 0f) return 0f;

        return GetCooldown(sushi) / interval;
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