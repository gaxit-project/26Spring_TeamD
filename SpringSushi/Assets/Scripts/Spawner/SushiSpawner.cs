using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("Prefab・データ")]
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;

    [Header("自動生成インターバル（秒）※生成のたびにリセット）")]
    public float autoSpawnInterval = 3.0f;

    [Header("手動生成インターバル（秒）")]
    public float manualSpawnInterval = 2.0f;

    // 現在選択中の寿司インデックス（SpawnerSelectorが操作）
    public int SelectedIndex { get; private set; } = 0;
    public SushiData SelectedSushi =>
        (sushiDataList != null && sushiDataList.Count > 0)
            ? sushiDataList[SelectedIndex]
            : null;

    // 手動生成クールダウン
    private float manualCooldownRemaining = 0f;
    public bool CanManualSpawn => manualCooldownRemaining <= 0f;

    private LaneNode myNode;
    private SushiRegistry registry;
    private Coroutine autoSpawnCoroutine;

    public void SetMasterNode(LaneNode master) => myNode = master;

    private void Start()
    {
        registry = FindFirstObjectByType<SushiRegistry>();
        if (myNode == null)
            myNode = GetComponent<LaneNode>();
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
            SpawnSushi(SelectedSushi);
        }
    }

    /// <summary>
    /// 手動生成（SpawnerSelectorから呼ばれる）
    /// </summary>
    public bool TryManualSpawn()
    {
        if (!CanManualSpawn) return false;

        SpawnSushi(SelectedSushi);
        manualCooldownRemaining = manualSpawnInterval;

        // 自動生成タイマーをリセット
        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        autoSpawnCoroutine = StartCoroutine(AutoSpawnRoutine());

        return true;
    }

    /// <summary>
    /// 寿司選択を左右に移動する（SpawnerSelectorから呼ばれる）
    /// </summary>
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

        // ★ ここで寿司の生成音（ポンッという音など）を鳴らす
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.SushiSpawn);
        }
    }

    /// <summary>
    /// スポーン位置のワールド座標（HUD表示用）
    /// </summary>
    public Vector3 WorldPosition => myNode != null ? myNode.Position : transform.position;
}