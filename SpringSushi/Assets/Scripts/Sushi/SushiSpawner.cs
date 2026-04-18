using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;
    public float spawnInterval = 3.0f;

    private LaneNode myNode;

    // --- LaneNetworkから呼び出される初期化関数 ---
    public void SetMasterNode(LaneNode master)
    {
        myNode = master;
        Debug.Log($"<color=green>[Spawner]</color> {name} が代表ノード {master.name} と同期しました。");
    }

    private void Start()
    {
        // バックアップ
        if (myNode == null) myNode = GetComponent<LaneNode>();

        StartCoroutine(SafeStart());
    }

    private IEnumerator SafeStart()
    {
        yield return null;

        if (myNode != null)
        {
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogError($"{name}: Nodeの紐付けに失敗したため、生成を開始できません。");
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSushi();
        }
    }

    private void SpawnSushi()
    {
        if (sushiBasePrefab == null || sushiDataList.Count == 0 || myNode == null)
        {
            Debug.LogWarning($"{name}: 生成条件不足");
            return;
        }

        LaneSegment targetSegment = FindOutgoingSegment();
        if (targetSegment == null)
        {
            Debug.LogWarning($"{name}: 有効な接続セグメントが見つからない");
            return;
        }

        // ★ 修正①：スポーン位置は常に myNode
        Vector3 spawnPosition = myNode.Position;

        GameObject sushiObj = Instantiate(sushiBasePrefab, spawnPosition, Quaternion.identity);

        SushiMovement movement = sushiObj.GetComponent<SushiMovement>();
        SushiData randomData = sushiDataList[Random.Range(0, sushiDataList.Count)];

        // ★ 修正②：spawnNode を渡す（これが最重要）
        movement.Initialize(randomData, targetSegment, myNode);
    }

    private LaneSegment FindOutgoingSegment()
    {
        foreach (var seg in myNode.connectedSegments)
        {
            // ★ ここはシンプルに「繋がってるものを返す」でOK
            return seg;
        }
        return null;
    }
}