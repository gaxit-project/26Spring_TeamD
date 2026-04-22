using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    [Header("Connection Info")]
    public List<LaneSegment> connectedSegments = new List<LaneSegment>();

    [Header("Branch Settings")]
    [Tooltip("分岐点として機能させるか")]
    public bool isBranch = false;
    private int nextOptionIndex = 0;

    public Vector3 Position => transform.position;

    /// <summary>
    /// 次に進むべきセグメントを選択する（分岐なら交互に）
    /// </summary>
    /// <param name="current">今いるセグメント</param>
    public LaneSegment GetNextSegment(LaneSegment current)
    {
        // 接続されているセグメントから、今来た道を除外する
        List<LaneSegment> options = new List<LaneSegment>();
        foreach (var seg in connectedSegments)
        {
            if (seg == current) continue;
            options.Add(seg);
        }

        if (options.Count == 0) return null;

        // 分岐設定がない、または道が1つしかないなら最初の道へ
        if (!isBranch || options.Count == 1)
        {
            return options[0];
        }

        // 分岐なら交互にインデックスを回す
        LaneSegment selected = options[nextOptionIndex % options.Count];
        nextOptionIndex++;

        Debug.Log($"<color=orange>[Branch]</color> {name}: 次の進路を {selected.name} に切り替えました。");
        return selected;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isBranch ? Color.yellow : Color.white;
        Gizmos.DrawWireSphere(Position, 0.2f);
    }
}