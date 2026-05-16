using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("Prefab・データ")]
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;

    [Header("自動生成インターバル（秒）※生成のたびにリセット")]
    public float autoSpawnInterval = 3.0f;

    [Header("手動生成インターバル（秒）")]
    public float manualSpawnInterval = 2.0f;

    public int SelectedIndex { get; private set; } = 0;
    public SushiData SelectedSushi =>
        (sushiDataList != null && sushiDataList.Count > 0)
            ? sushiDataList[SelectedIndex] : null;

    private float manualCooldownRemaining = 0f;
    public bool CanManualSpawn => manualCooldownRemaining <= 0f;

    private LaneNode myNode;
    private SushiRegistry registry;
    private Coroutine autoSpawnCoroutine;

    // ★ 追加: 自動生成を一時的にスキップするフラグ
    private bool suppressNextAutoSpawn = false;

    public void SetMasterNode(LaneNode master) => myNode = master;

    private void Start()
    {
        registry = FindFirstObjectByType<SushiRegistry>();
        if (myNode == null) myNode = GetComponent<LaneNode>();
        StartCoroutine(SafeStart());
    }

    private void Update()
    {
        if (manualCooldownRemaining > 0f)
            manualCooldownRemaining -= Time.deltaTime;
    }

    private IEnumerator SafeStart()
    {
        yield return null;
        if (myNode != null)
            autoSpawnCoroutine = StartCoroutine(AutoSpawnRoutine());
    }

    private IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSpawnInterval);

            // ★ 手動生成と被りそうなときはスキップしてタイマーをリセット
            if (suppressNextAutoSpawn)
            {
                suppressNextAutoSpawn = false;
                continue; // while(true)の先頭へ戻りタイマー再計測
            }

            SpawnSushi(SelectedSushi);
        }
    }

    public bool TryManualSpawn()
    {
        if (!CanManualSpawn) return false;

        // ★ 次の自動生成をスキップ予約してからCoroutineをリセット
        suppressNextAutoSpawn = true;

        SpawnSushi(SelectedSushi);
        manualCooldownRemaining = manualSpawnInterval;

        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        autoSpawnCoroutine = StartCoroutine(AutoSpawnRoutine());
        return true;
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