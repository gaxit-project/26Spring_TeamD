using System.Collections.Generic;
using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data; // まぐろ、たまご等のアセット

    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public LaneSegment currentSegment; // 今いるセグメント（重要！）
    [Range(0, 1)] public float progress = 0f;

    private GameObject currentModel;
    private bool lastReversedState; // 前フレームの反転状態を記憶

    /// <summary>
    /// 寿司の初期設定（生成時に呼ぶ）
    /// </summary>
    public void Initialize(SushiData newData, LaneSegment startSegment)
    {
        data = newData;
        currentSegment = startSegment;
        progress = 0f;

        // 見た目の生成
        if (data != null && data.sushiModel != null) // data自体のNullチェックを追加
        {
            currentModel = Instantiate(data.sushiModel, transform);
        }
        else
        {
            // データがない場合は警告を出す（デバッグ用）
            Debug.LogWarning($"{name}: SushiData または modelPrefab が設定されていません。見た目は生成されません。");
        }
    }

    private void Update()
    {
        if (currentSegment == null) return;

        // --- 反転検知ロジック ---
        // セグメントの反転状態が変わったら、progressを反転させる
        if (currentSegment.isReversed != lastReversedState)
        {
            progress = 1.0f - progress;
            lastReversedState = currentSegment.isReversed;
        }

        // 移動進捗の計算
        // nodeA, nodeB が実行時に Network によって割り当てられている必要がある
        if (currentSegment.nodeA == null || currentSegment.nodeB == null) return;

        float distance = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);

        // 距離が0（Nodeが重なっている）の場合は移動しない（ゼロ除算防止）
        if (distance > 0.001f)
        {
            progress += (moveSpeed / distance) * Time.deltaTime;
        }

        // 2. 位置の更新（IsReversedを考慮した入口/出口の自動取得）
        UpdatePosition();

        // 3. 次のセグメントへの遷移
        if (progress >= 1.0f)
        {
            currentSegment = currentSegment.GetNextSegment();
            progress = 0f;

            if (currentSegment == null) SushiDestroy(); // 行き止まり
        }
    }

    private void UpdatePosition()
    {
        // GetEntryNode, GetExitNode が内部で nodeA, nodeB を正しく参照している前提
        if (currentSegment == null) return;

        Vector3 start = currentSegment.GetEntryNode().Position;
        Vector3 end = currentSegment.GetExitNode().Position;
        transform.position = Vector3.Lerp(start, end, progress);

        // 進行方向を向く
        Vector3 dir = end - start;
        if (dir != Vector3.zero) transform.forward = dir;
    }

    /// <summary>
    /// 寿司をシーンから消滅させる（一括管理）
    /// </summary>
    public void SushiDestroy()
    {
        // ここにエフェクト生成（ネタが飛び散る等）を将来的に追加可能
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // --- 1. 寿司の実体を表す赤い球体（今回追加） ---
        // 実行中かどうかに関わらず、このコンポーネントがついている位置に描画
        Gizmos.color = Color.red;
        // 半径は Collider のサイズに合わせると、より直感的になります（ここでは例として0.3f）
        Gizmos.DrawSphere(transform.position, 0.3f);


        // --- 2. 進行状況のテキストと進行方向線（以前の実装を少し強化） ---
        if (!Application.isPlaying || currentSegment == null) return;

        // 寿司の真上に情報を表示（Sceneビューのみ）
#if UNITY_EDITOR
        string sushiName = (data != null) ? data.sushiName : "No Data";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f,
            $"[{sushiName}]\nProgress: {progress:P0}\nSeg: {currentSegment.name}");
#endif

        // 現在ターゲットにしている出口Nodeを線で結ぶ
        Gizmos.color = Color.yellow;
        Vector3 exitPos = currentSegment.GetExitNode().Position;
        Gizmos.DrawLine(transform.position, exitPos);
        // 出口Node自体にも小さな黄色い球を描画
        Gizmos.DrawWireSphere(exitPos, 0.2f);
    }

    // 衝突判定（Triggerを使用する場合）
    private void OnTriggerEnter(Collider other)
    {
        // 相手も寿司なら両方消滅
        if (other.CompareTag("Sushi"))
        {
            string myName = (data != null) ? data.sushiName : gameObject.name;
            Debug.Log($"<color=red>衝突消滅!</color> {myName} が {other.gameObject.name} とぶつかりました。");
            SushiDestroy();
            // 相手側は相手のOnTriggerEnterで自爆するのでここで相手を消さなくてOK
        }
    }
}