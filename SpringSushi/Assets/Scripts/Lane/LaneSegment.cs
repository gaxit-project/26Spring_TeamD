using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSegment : MonoBehaviour
{
    [Header("設定")]
    public LaneColor laneColor;


    [Header("接続ポイント (2つアサインしてください)")]
    public Transform[] connectionPoints;

    //実行時にManagerから割り当てられる接続情報
    [HideInInspector] public LaneNode nodeA;// 端点1
    [HideInInspector] public LaneNode nodeB;// 端点2

    public bool isReversed = false;

    /// <summary>
    /// 現在の進行方向における「出口」のNodeを返す(反転しているならA地点(B→A)を、反転していないならB地点(A→B)を『出口』として採用する)関数
    /// </summary>
    public LaneNode GetExitNode() => isReversed ? nodeA : nodeB;

    /// <summary>
    /// 現在の進行方向における「入口」のNodeを返す(反転しているならB地点(B→A)を、反転していないならA地点(A→B)を『入り口』として採用する)関数
    /// </summary>
    public LaneNode GetEntryNode() => isReversed ? nodeB : nodeA;
   
    ///<summary>
    ///Managerから呼ばれたら進行方向を反転させる関数
    /// </summary>
    public void Reverse()
    {
        isReversed = !isReversed;
        GetComponent<Renderer>().material.color = isReversed ? Color.black : Color.white;　//←この1行はデバック用
　　}

    ///<summary>
    ///この辺(segment)を通過した後、次の辺が繋がっているか探す関数
    /// </summary>
    public LaneSegment GetNextSegment()
    {
        LaneNode exitNode = GetExitNode();//出口nodeをローカル変数で記憶
        if (exitNode == null) return null;

        foreach(var segment in exitNode.connectedSegments) //自分と繋がっている全てのnodeを
        {
            if (segment == this) continue; //自分自身を除外

            if (segment.GetEntryNode() == exitNode)
            {
                return segment;
            }
        }
        return null;
    }
}
