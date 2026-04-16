using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    public List<LaneSegment> allSegments = new List<LaneSegment>();
    // snapThresholdは座標統合の判定距離として使います
    [SerializeField] private float snapThreshold = 0.1f;

    private void Awake()
    {
        InitializeGraph();
    }

    private void InitializeGraph()
    {
        // シーン内の全セグメントを取得
        allSegments = FindObjectsByType<LaneSegment>(FindObjectsSortMode.None).ToList();

        // 座標ベースで代表Nodeを管理する辞書
        Dictionary<Vector3, LaneNode> masterNodeMap = new Dictionary<Vector3, LaneNode>();

        foreach (var seg in allSegments)
        {
            // 子オブジェクトからLaneNodeコンポーネントを2つ探す
            LaneNode[] childNodes = seg.GetComponentsInChildren<LaneNode>();

            if (childNodes.Length < 2)
            {
                Debug.LogWarning($"{seg.name} の子にNodeが2つありません。無視します。");
                continue;
            }

            // 座標が重なっているNode同士を「代表」に集約して代入
            seg.nodeA = GetOrCreateMasterNode(childNodes[0], masterNodeMap);
            seg.nodeB = GetOrCreateMasterNode(childNodes[1], masterNodeMap);

            // 代表Node側にこのセグメントを登録
            if (!seg.nodeA.connectedSegments.Contains(seg)) seg.nodeA.connectedSegments.Add(seg);
            if (!seg.nodeB.connectedSegments.Contains(seg)) seg.nodeB.connectedSegments.Add(seg);
        }
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

    // ToggleLanesなどの関数はそのまま
    private void OnEnable() => LaneInputManager.OnLaneButtonPressed += ToggleLanes;
    private void OnDisable() => LaneInputManager.OnLaneButtonPressed -= ToggleLanes;

    public void ToggleLanes(LaneColor color)
    {
        foreach (var seg in allSegments)
        {
            if (seg.laneColor == color) seg.Reverse();
        }
    }
}