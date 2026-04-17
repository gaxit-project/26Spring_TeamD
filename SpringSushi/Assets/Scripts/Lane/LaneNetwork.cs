using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    public List<LaneSegment> allSegments = new List<LaneSegment>();
    [SerializeField] private float snapThreshold = 0.1f; // 現在はRound計算に使用

    private void Awake()
    {
        InitializeGraph();
    }

    // --- 入力イベントの購読 ---
    private void OnEnable()
    {
        // LaneInputManagerからの通知を受け取る設定
        LaneInputManager.OnLaneButtonPressed += ToggleLanes;
    }

    private void OnDisable()
    {
        // メモリリーク防止のため解除
        LaneInputManager.OnLaneButtonPressed -= ToggleLanes;
    }

    public void ToggleLanes(LaneColor color)
    {
        foreach (var seg in allSegments)
        {
            // 色が一致するセグメントを反転
            if (seg.laneColor == color)
            {
                seg.Reverse();
            }
        }
        Debug.Log($"<color=cyan>[Network]</color> {color} レーンの向きを反転しました。");
    }

    private void InitializeGraph()
    {
        allSegments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();

        // シーン内の全Node（独立したSpawnerNode含む）をリストアップ
        List<LaneNode> allNodesInScene = FindObjectsByType<LaneNode>(FindObjectsSortMode.None).ToList();
        Dictionary<Vector3, LaneNode> masterNodeMap = new Dictionary<Vector3, LaneNode>();

        // 1. 全てのNodeを座標ごとに「代表（Master）」へ集約
        foreach (var node in allNodesInScene)
        {
            GetOrCreateMasterNode(node, masterNodeMap);
        }

        // 2. セグメントに「代表Node」を割り当て、接続リストを構築
        foreach (var seg in allSegments)
        {
            LaneNode[] childNodes = seg.GetComponentsInChildren<LaneNode>();
            if (childNodes.Length < 2) continue;

            // 代表ノードに置き換える
            seg.nodeA = GetOrCreateMasterNode(childNodes[0], masterNodeMap);
            seg.nodeB = GetOrCreateMasterNode(childNodes[1], masterNodeMap);

            // 代表ノード側にこのセグメントを登録
            if (!seg.nodeA.connectedSegments.Contains(seg)) seg.nodeA.connectedSegments.Add(seg);
            if (!seg.nodeB.connectedSegments.Contains(seg)) seg.nodeB.connectedSegments.Add(seg);
        }

        // 3. 【重要】独立したSpawnerなどにあるLaneNodeも「代表」を参照するように上書き
        // これをしないと、Spawnerが持っているmyNodeが孤立したままになります
        foreach (var node in allNodesInScene)
        {
            // すでに自分が代表でない場合、中身を代表の接続リストと同期させる
            LaneNode master = GetOrCreateMasterNode(node, masterNodeMap);
            if (node != master)
            {
                // 独立したNodeの参照リストを、統合された代表Nodeのリストと共有させる
                node.connectedSegments = master.connectedSegments;
            }
        }

        Debug.Log($"<color=green>[Network]</color> グラフ初期化完了: {allSegments.Count}セグメント / {masterNodeMap.Count}接点");
    }

    private LaneNode GetOrCreateMasterNode(LaneNode original, Dictionary<Vector3, LaneNode> map)
    {
        // 座標を丸めて微細なズレを許容する
        Vector3 key = new Vector3(
            Mathf.Round(original.Position.x * 100f) / 100f,
            Mathf.Round(original.Position.y * 100f) / 100f,
            Mathf.Round(original.Position.z * 100f) / 100f
        );

        if (map.ContainsKey(key)) return map[key];

        map.Add(key, original);
        return original;
    }
}