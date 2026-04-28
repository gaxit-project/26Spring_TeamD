using UnityEngine;

/// <summary>
/// LaneNodeの見た目を管理する。
/// IsReversedの変化を受けて、矢印プレートを現在の出口方向へ回転させる。
///
/// 【Prefab構成の想定】
/// LaneNode (GameObject)
///   └ NodePlate (GameObject) ← このスクリプトをアタッチ
///       └ 矢印が描かれた丸いプレートのMesh（矢印はローカルZ+方向を向いている想定）
/// </summary>
public class LaneNodeView : MonoBehaviour
{
    [SerializeField] private LaneNode node;

    [Header("回転設定")]
    [Tooltip("矢印プレートのTransform。nullの場合はこのGameObject自身を使う")]
    [SerializeField] private Transform arrowPlate;

    [Tooltip("回転をなめらかにする速さ。0にするとスナップ回転")]
    [SerializeField] private float rotateSpeed = 10f;

    private Quaternion targetRotation;

    private void Awake()
    {
        if (node == null)
            node = GetComponentInParent<LaneNode>();

        if (arrowPlate == null)
            arrowPlate = transform;

        node.OnReversedChanged += OnReversedChanged;

        // 初期方向を反映
        UpdateTargetRotation();
        arrowPlate.rotation = targetRotation;
    }

    private void Update()
    {
        if (rotateSpeed <= 0f)
        {
            arrowPlate.rotation = targetRotation;
            return;
        }
        arrowPlate.rotation = Quaternion.Slerp(arrowPlate.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void OnReversedChanged(bool isReversed)
    {
        UpdateTargetRotation();
    }

    private void UpdateTargetRotation()
    {
        LaneSegment exitSeg = node.GetExitSegment();
        if (exitSeg == null) return;

        // 出口Segmentのどちら方向へ進むかを求める
        Vector3 exitDirection = GetExitDirection(exitSeg);
        if (exitDirection == Vector3.zero) return;

        // 矢印をその方向に向ける（Y軸回転のみ。プレートが水平に置かれている想定）
        targetRotation = Quaternion.LookRotation(exitDirection, Vector3.up);
    }

    private Vector3 GetExitDirection(LaneSegment seg)
    {
        if (seg.nodeA == null || seg.nodeB == null) return Vector3.zero;

        // このNodeから見た出口方向を返す
        // IsReversed=false → nodeAからnodeBへ流れる → nodeがnodeAなら前方はnodeB方向
        if (seg.nodeA == node)
            return (seg.nodeB.Position - seg.nodeA.Position).normalized;
        else if (seg.nodeB == node)
            return (seg.nodeA.Position - seg.nodeB.Position).normalized;

        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (node != null)
            node.OnReversedChanged -= OnReversedChanged;
    }
}