using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レーンの接合点（グラフ構造のノード）
/// 複数のLaneSegmentを接続し、寿司の進路の分岐・合流を司る
/// </summary>
public class LaneNode : MonoBehaviour
{
    [Header("Connection Info")]
    [Tooltip("このNodeに接続されている全セグメントのリスト")]
    public List<LaneSegment> connectedSegments = new List<LaneSegment>();

    /// <summary>
    /// Nodeのワールド座標を返す
    /// </summary>
    public Vector3 Position => transform.position;

    /// <summary>
    /// ギズモを表示してエディタ上での視認性を高める
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(Position, 0.2f);
    }
}