using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    public float moveSpeed = 1.0f;

    public LaneSegment currentSegment;
    private float progress = 0f; // 0.0(nodeA) ～ 1.0(nodeB)
    private bool movingTowardsNodeB = true; // true: A->B(0->1), false: B->A(1->0)

    // ★ 追記：Networkの参照を保持
    private LaneNetwork network;

    public void Initialize(SushiData data, LaneSegment seg, LaneNode spawnNode)
    {
        // ★ 追記：Networkの参照を取得してリストに自分を登録
        network = Object.FindFirstObjectByType<LaneNetwork>();
        if (network != null) network.RegisterSushi(this);

        this.data = data;
        currentSegment = seg;

        // 初期の進行方向をレーンの流れに合わせて決定
        UpdateMovingDirection();

        // スポーン位置：NodeAから入ったなら0、NodeBなら1
        progress = (spawnNode == seg.nodeA) ? 0f : 1f;

        if (data?.sushiModel != null)
        {
            GameObject model = Instantiate(data.sushiModel, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (currentSegment == null) return;

        Move();

        // 判定：目的地(AかBか)に到達したか
        if ((movingTowardsNodeB && progress >= 1.0f) || (!movingTowardsNodeB && progress <= 0.0f))
        {
            SwitchToNextSegment();
        }
    }

    private void OnDestroy()
    {
        // ★ 追記：破棄される時にリストから自分を外す
        if (network != null)
        {
            network.UnregisterSushi(this);
        }
    }

    public void Move()
    {
        float totalDist = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);
        if (totalDist <= 0.001f) return;

        // 自分の目的地に従ってprogressを増減
        float directionFactor = movingTowardsNodeB ? 1f : -1f;
        progress += (moveSpeed / totalDist) * Time.deltaTime * directionFactor;

        // 描画位置の計算
        float t = Mathf.Clamp01(progress);
        transform.position = Vector3.Lerp(currentSegment.nodeA.Position, currentSegment.nodeB.Position, t);

        // 向きの更新
        Vector3 forwardVec = currentSegment.nodeB.Position - currentSegment.nodeA.Position;
        if (forwardVec != Vector3.zero)
        {
            transform.forward = forwardVec * directionFactor;
        }
    }

    /// <summary>
    /// 今のレーンのisReversedを見て、自分がAとBどっちに向かうべきか更新する
    /// </summary>
    public void UpdateMovingDirection()
    {
        // isReversed=false(通常)ならB(1.0)へ、true(反転)ならA(0.0)へ
        movingTowardsNodeB = !currentSegment.isReversed;
    }

    /// <summary>
    /// LaneNetworkから呼ばれる反転同期用
    /// </summary>
    public void SyncDirectionWithSegment()
    {
        // レーンが反転した瞬間に、自分がAとBどっちに向かうべきかを即座に書き換える
        UpdateMovingDirection();

        // デバッグログ（動いたら消してOK）
        Debug.Log($"<color=yellow>[Reverse Sync]</color> {gameObject.name} の進行方向が反転しました。");
    }

    private void SwitchToNextSegment()
    {
        // 到着したノードを目的地フラグから特定
        LaneNode arrivalNode = movingTowardsNodeB ? currentSegment.nodeB : currentSegment.nodeA;

        // Nodeに「次に行ける(逆走でない)道」を聞く
        LaneSegment next = arrivalNode.GetNextSegment(currentSegment);

        if (next != null)
        {
            currentSegment = next;

            // 新しいレーンの流れに自分の意志を合わせる
            UpdateMovingDirection();

            // 進入位置のセット
            progress = (arrivalNode == next.nodeA) ? 0f : 1f;
        }
        else
        {
            // 行き先がない（行き止まり、または全ての接続先が逆流）
            Debug.Log($"<color=red>[DeadEnd]</color> {gameObject.name} の行き先がありません。");
            SushiDestroy();
        }
    }

    public void SushiDestroy() => Destroy(gameObject);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sushi"))
        {
            SushiDestroy();
        }
    }
}