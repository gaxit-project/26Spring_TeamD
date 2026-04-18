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
        // ...既存のNullチェック等はそのまま...

        LaneSegment targetSegment = FindOutgoingSegment();
        if (targetSegment != null)
        {
            // 【修正ポイント】
            // transform.position ではなく、送り出すレーンの EntryNode の座標を取得する
            Vector3 spawnPosition = targetSegment.GetEntryNode().Position;

            // 生成位置を EntryNode に指定する
            GameObject sushiObj = Instantiate(sushiBasePrefab, spawnPosition, Quaternion.identity);

            SushiMovement movement = sushiObj.GetComponent<SushiMovement>();
            SushiData randomData = sushiDataList[Random.Range(0, sushiDataList.Count)];

            // Progress 0 から開始
            movement.Initialize(randomData, targetSegment);
        }
        // ...省略...
    }

    private LaneSegment FindOutgoingSegment()
    {
        // 今の myNode に繋がっているセグメントを調べる
        foreach (var seg in myNode.connectedSegments)
        {
            // ヒエラルキー上に存在する「本物」のセグメントか確認
            // (もしプレハブを参照していたら、シーン上の本物に差し替える)
            if (seg.GetEntryNode() == myNode) return seg;
        }
        return null;
    }
}