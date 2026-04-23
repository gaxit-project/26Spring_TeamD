using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaneNetwork : MonoBehaviour
{
    public List<LaneSegment> allSegments = new List<LaneSegment>();
    private List<SushiMovement> activeSushiList = new List<SushiMovement>();
    [SerializeField] private float snapThreshold = 0.1f;

    // レーン状態管理（Key: 色, Value: 反転しているか）
    private Dictionary<LaneColor, bool> laneStates = new Dictionary<LaneColor, bool>();

    private void Awake()
    {
        InitializeGraph();
    }

    private void OnEnable()
    {
        LaneInputManager.OnLaneButtonPressed += ToggleLanes;
    }

    private void OnDisable()
    {
        LaneInputManager.OnLaneButtonPressed -= ToggleLanes;
    }

    // ★ 寿司が生成された時に呼ぶ
    public void RegisterSushi(SushiMovement sushi)
    {
        if (!activeSushiList.Contains(sushi))
            activeSushiList.Add(sushi);
    }

    // ★ 寿司が破棄された時に呼ぶ
    public void UnregisterSushi(SushiMovement sushi)
    {
        if (activeSushiList.Contains(sushi))
            activeSushiList.Remove(sushi);
    }

    public void ToggleLanes(LaneColor color)
    {
        // ★ 追記：状態の反転処理
        if (!laneStates.ContainsKey(color)) laneStates[color] = false;
        laneStates[color] = !laneStates[color];

        bool newState = laneStates[color];

        foreach (var seg in allSegments)
        {
            if (seg.laneColor == color)
            {
                seg.SetReversed(newState);

                // ★ 高速化：FindObjectsByType を使わず、管理リストを逆順走査する
                for (int i = activeSushiList.Count - 1; i >= 0; i--)
                {
                    // 万が一、Destroy のタイミングで null になっている場合のガード
                    if (activeSushiList[i] == null)
                    {
                        activeSushiList.RemoveAt(i);
                        continue;
                    }

                    if (activeSushiList[i].currentSegment == seg)
                    {
                        activeSushiList[i].SyncDirectionWithSegment();
                    }
                }
            }
        }
    }

    private void InitializeGraph()
    {
        allSegments = FindObjectsByType<LaneSegment>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();

        List<LaneNode> allNodesInScene = FindObjectsByType<LaneNode>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        Dictionary<Vector3, LaneNode> masterNodeMap = new Dictionary<Vector3, LaneNode>();

        foreach (var node in allNodesInScene)
        {
            GetOrCreateMasterNode(node, masterNodeMap);
        }

        foreach (var seg in allSegments)
        {
            LaneNode[] childNodes = seg.GetComponentsInChildren<LaneNode>();
            if (childNodes.Length < 2) continue;

            seg.nodeA = GetOrCreateMasterNode(childNodes[0], masterNodeMap);
            seg.nodeB = GetOrCreateMasterNode(childNodes[1], masterNodeMap);

            if (!seg.nodeA.connectedSegments.Contains(seg)) seg.nodeA.connectedSegments.Add(seg);
            if (!seg.nodeB.connectedSegments.Contains(seg)) seg.nodeB.connectedSegments.Add(seg);

            Debug.Log($"[Network] {seg.name} に MasterNode を割り当てました。");
        }

        SushiSpawner[] spawners = FindObjectsByType<SushiSpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            Vector3 key = new Vector3(
                Mathf.Round(spawner.transform.position.x * 100f) / 100f,
                Mathf.Round(spawner.transform.position.y * 100f) / 100f,
                Mathf.Round(spawner.transform.position.z * 100f) / 100f
            );

            if (masterNodeMap.ContainsKey(key))
            {
                spawner.SetMasterNode(masterNodeMap[key]);
            }
        }

        Debug.Log($"<color=green>[Network]</color> グラフ初期化完了: {allSegments.Count}セグメント / {masterNodeMap.Count}接点");
    }

    private LaneNode GetOrCreateMasterNode(LaneNode original, Dictionary<Vector3, LaneNode> map)
    {
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