using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("秒速（ユニット/秒）")]
    public float moveSpeed = 1.0f;

    [Header("現在のステート")]
    public LaneSegment currentSegment;
    [Range(0, 1)] public float progress = 0f; // 0.0 (入口) ～ 1.0 (出口)

    /// <summary>
    /// 寿司を特定のレーンからスタートさせる初期化関数
    /// </summary>
    public void Initialize(LaneSegment startSegment)
    {
        currentSegment = startSegment;
        progress = 0f;
        UpdateTransform();
    }

    private void Update()
    {
        if (currentSegment == null) return;

        // 1. 移動進捗の計算
        // セグメントの長さ（距離）を取得して、移動量を正規化（0～1）する
        float distance = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);

        // Time.deltaTimeを使用してフレームレートに依存しない移動を実現
        // 進行方向がどちらであっても、progressは「出口」に向かって増えていく設計
        progress += (moveSpeed / distance) * Time.deltaTime;

        // 2. 座標と回転の更新
        UpdateTransform();

        // 3. 端（出口）に到達した時の処理
        if (progress >= 1.0f)
        {
            SwitchToNextSegment();
        }
    }

    /// <summary>
    /// progress（0～1）に基づいて実際の座標を計算・反映する
    /// </summary>
    private void UpdateTransform()
    {
        // 常に「今の」入口と出口を取得する（逆転に対応）
        Vector3 entryPos = currentSegment.GetEntryNode().Position;
        Vector3 exitPos = currentSegment.GetExitNode().Position;

        // 線形補間で現在地を計算
        transform.position = Vector3.Lerp(entryPos, exitPos, progress);

        // 進行方向を向く（オプション）
        Vector3 direction = exitPos - entryPos;
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }
    }

    /// <summary>
    /// 次のセグメントへ乗り換える
    /// </summary>
    private void SwitchToNextSegment()
    {
        LaneSegment next = currentSegment.GetNextSegment();

        if (next != null)
        {
            // 次のレーンへバトンタッチ
            currentSegment = next;
            progress = 0f; // 新しいレーンの入口(0)からスタート
        }
        else
        {
            // 次のレーンがない（行き止まり）
            Debug.Log($"{gameObject.name}: 行き止まりに到達しました。");
            // 提供成功処理、または脱落処理
            Destroy(gameObject);
        }
    }
}