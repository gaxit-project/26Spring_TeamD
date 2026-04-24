using System.Collections.Generic;
using UnityEngine;

public class LaneNode : MonoBehaviour
{
    [Header("Connection Info")]
    public List<LaneSegment> connectedSegments = new List<LaneSegment>();

    [Header("Branch Settings")]
    [Tooltip("分岐点として機能させるか（交互切り替え）")]
    public bool isBranch = false;
    private int nextOptionIndex = 0;

    public Vector3 Position => transform.position;

    /// <summary>
    /// 次に進むべきセグメントを選択する（逆流している道は除外する）
    /// </summary>
    public LaneSegment GetNextSegment(LaneSegment current)
    {
        List<LaneSegment> options = new List<LaneSegment>();

        foreach (var seg in connectedSegments)
        {
            if (seg == current) continue;

            // ★ 重要：その道が今「入口」として機能しているかチェック
            if (IsEnterable(seg, this))
            {
                options.Add(seg);
            }
        }

        if (options.Count == 0) return null;

        // 候補が1つならそれを、複数なら交互に返す
        if (!isBranch || options.Count == 1)
        {
            return options[0];
        }

        LaneSegment selected = options[nextOptionIndex % options.Count];
        nextOptionIndex++;

        Debug.Log($"<color=orange>[Branch]</color> {name}: {selected.name} へ分岐しました。");
        return selected;
    }

    /// <summary>
    /// 指定したノードからそのセグメントに「順走」で進入できるか判定
    /// </summary>
    private bool IsEnterable(LaneSegment seg, LaneNode fromNode)
    {
        // 通常(false): Aから入りたい / 反転(true): Bから入りたい
        if (!seg.IsReversed)
        {
            return fromNode == seg.nodeA;
        }
        else
        {
            return fromNode == seg.nodeB;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isBranch ? Color.yellow : Color.white;
        Gizmos.DrawWireSphere(Position, 0.2f);
    }
}