using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject sushiBasePrefab; // Sushi_Baseプレハブ
    public List<SushiData> sushiDataList; // 流す寿司の候補
    public float spawnInterval = 3.0f;

    private LaneNode myNode;

    private void Start()
    {
        myNode = GetComponent<LaneNode>();
        StartCoroutine(SafeStart());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSushi();
        }
    }
    private IEnumerator SafeStart()
    {
        yield return null; // Networkの初期化完了を確実に待つ
        StartCoroutine(SpawnRoutine());
    }

    private void SpawnSushi()
    {
        if (myNode == null) return;

        // デバッグログ：このNodeが認識しているレーンの数
        int count = myNode.connectedSegments.Count;
        Debug.Log($"<color=cyan>[Spawner Debug]</color> 接続されているレーン数: {count}");

        foreach (var seg in myNode.connectedSegments)
        {
            // ログ：各レーンの今の入口が誰かを表示
            LaneNode entry = seg.GetEntryNode();
            Debug.Log($"<color=yellow>[Lane Check]</color> レーン名: {seg.name} | 現在の入口: {entry.name} | Spawnerと一致? : {entry == myNode}");
        }

        // --- 既存の生成ロジック ---
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
            Debug.LogWarning("送り出せるレーンがありません。");
        }
    }

    /// <summary>
    /// 自分(Node)から出ていく方向に設定されているセグメントを1つ返す
    /// </summary>
    private LaneSegment FindOutgoingSegment()
    {
        foreach (var seg in myNode.connectedSegments)
        {
            // セグメントの「入口」がこのNodeなら、そこは「出口」として使える
            if (seg.GetEntryNode() == myNode)
            {
                return seg;
            }
        }
        return null;
    }
}