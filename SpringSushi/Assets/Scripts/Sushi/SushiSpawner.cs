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
        // 万が一Networkから呼ばれなかった時のためのバックアップ
        if (myNode == null) myNode = GetComponent<LaneNode>();

        StartCoroutine(SafeStart());
    }

    private IEnumerator SafeStart()
    {
        // 1フレーム待機してNetworkの初期化(InitializeGraph)完了を待つ
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
        if (myNode == null) return;

        // デバッグログ
        int count = myNode.connectedSegments.Count;
        Debug.Log($"<color=cyan>[Spawner Debug]</color> 接続数: {count}");

        foreach (var seg in myNode.connectedSegments)
        {
            LaneNode entry = seg.GetEntryNode();
            Debug.Log($"<color=yellow>[Check]</color> {seg.name} | 入口: {entry.name} | Spawnerと一致? : {entry == myNode}");
        }

        LaneSegment targetSegment = FindOutgoingSegment();
        if (targetSegment != null)
        {
            GameObject sushiObj = Instantiate(sushiBasePrefab, transform.position, Quaternion.identity);
            SushiMovement movement = sushiObj.GetComponent<SushiMovement>();
            SushiData randomData = sushiDataList[Random.Range(0, sushiDataList.Count)];
            movement.Initialize(randomData, targetSegment);
        }
        else
        {
            Debug.LogWarning("送り出せるレーンがありません。向きを確認してください。");
        }
    }

    private LaneSegment FindOutgoingSegment()
    {
        foreach (var seg in myNode.connectedSegments)
        {
            if (seg.GetEntryNode() == myNode) return seg;
        }
        return null;
    }
}