using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    public float moveSpeed = 1.0f;

    [Header("振動設定")]
    public float wobbleAmplitude = 0.15f;
    public float wobbleFrequency = 8f;

    public LaneSegment currentSegment;

    private float progress = 0f;
    private bool movingTowardsNodeB = true;
    private SushiRegistry registry;

    // --- 停滞ステート ---
    private bool isStuck = false;
    private LaneNode stuckNode;
    private LaneSegment arrivedFromSegment;
    private Vector3 wobbleAxis;
    private float wobbleTime = 0f;

    public void Initialize(SushiData data, LaneSegment seg, LaneNode spawnNode, SushiRegistry registry)
    {
        this.registry = registry;
        registry?.Register(this);
        this.data = data;
        currentSegment = seg;
        UpdateMovingDirection();
        progress = (spawnNode == seg.nodeA) ? 0f : 1f;

        if (data?.sushiModel != null)
        {
            GameObject model = Instantiate(data.sushiModel, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnDestroy()
    {
        registry?.Unregister(this);
    }

    private void Update()
    {
        if (isStuck)
        {
            UpdateWobble();
            return;
        }

        if (currentSegment == null) return;
        Move();

        if ((movingTowardsNodeB && progress >= 1.0f) || (!movingTowardsNodeB && progress <= 0.0f))
        {
            SwitchToNextSegment();
        }
    }

    public void Move()
    {
        float totalDist = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);
        if (totalDist <= 0.001f) return;

        float directionFactor = movingTowardsNodeB ? 1f : -1f;
        progress += (moveSpeed / totalDist) * Time.deltaTime * directionFactor;
        float t = Mathf.Clamp01(progress);
        transform.position = Vector3.Lerp(currentSegment.nodeA.Position, currentSegment.nodeB.Position, t);

        Vector3 forwardVec = currentSegment.nodeB.Position - currentSegment.nodeA.Position;
        if (forwardVec != Vector3.zero)
            transform.forward = forwardVec * directionFactor;
    }

    public void UpdateMovingDirection()
    {
        movingTowardsNodeB = !currentSegment.IsReversed;
    }

    public void SyncDirectionWithSegment()
    {
        if (isStuck) return;
        UpdateMovingDirection();
    }

    private void SwitchToNextSegment()
    {
        LaneNode arrivalNode = movingTowardsNodeB ? currentSegment.nodeB : currentSegment.nodeA;
        LaneSegment next = arrivalNode.GetNextSegment(currentSegment);

        if (next != null)
        {
            currentSegment = next;
            UpdateMovingDirection();
            progress = (arrivalNode == next.nodeA) ? 0f : 1f;
        }
        else
        {
            EnterStuck(arrivalNode, currentSegment);
        }
    }

    // --- 停滞処理 ---

    private void EnterStuck(LaneNode node, LaneSegment from)
    {
        isStuck = true;
        stuckNode = node;
        arrivedFromSegment = from;
        wobbleTime = 0f;

        // 振動軸：到着したSegmentのA→B方向
        Vector3 dir = from.nodeB.Position - from.nodeA.Position;
        wobbleAxis = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.right;

        transform.position = node.Position;
        Debug.Log($"<color=yellow>【Stuck】</color> {gameObject.name} が {node.name} で停滞");
    }

    private void UpdateWobble()
    {
        wobbleTime += Time.deltaTime;
        float offset = Mathf.Sin(wobbleTime * wobbleFrequency) * wobbleAmplitude;
        transform.position = stuckNode.Position + wobbleAxis * offset;
    }

    public void SushiDestroy() => Destroy(gameObject);

    /// <summary>
    /// LaneNetworkから呼ばれる。振動位置（どちらに寄っているか）に基づいて再出発するSegmentを決める。
    /// </summary>
    public void TryExitStuck()
    {
        if (!isStuck) return;

        // 現在の exit Segment を取得（arrivedFrom との一致チェックは行わない）
        LaneSegment exitSeg = stuckNode.GetExitSegment();
        if (exitSeg == null) return;

        // exit Segment に stuckNode 側から乗れるか確認
        // （IsReversed=false なら nodeA から乗る、true なら nodeB から乗る）
        bool canEnter = exitSeg.IsReversed
            ? exitSeg.nodeB == stuckNode
            : exitSeg.nodeA == stuckNode;
        if (!canEnter) return;

        isStuck = false;
        currentSegment = exitSeg;
        UpdateMovingDirection();
        progress = (stuckNode == exitSeg.nodeA) ? 0f : 1f;
        transform.position = stuckNode.Position;

        Debug.Log($"<color=cyan>【Unstuck】</color> {gameObject.name} → {exitSeg.name}");
        stuckNode = null;
        arrivedFromSegment = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sushi"))
        {
            // 衝突した寿司の合計金額を減算
            var otherSushi = other.GetComponent<SushiMovement>();
            int lossAmount = data.price + (otherSushi != null ? otherSushi.data.price : 0);
            ScoreManager.Instance?.SubtractScore(lossAmount);
            PricePopupManager.Instance?.ShowPopup(-lossAmount, transform.position);

            // ★ ここで皿が割れる音を鳴らす
            if (SoundPlayer.Instance != null)
            {
                SoundPlayer.Instance.PlaySFX(SoundKeys.PlateBreak);
            }

            // 両方をDestroy（相手側のOnTriggerEnterが二重発火しないようにnullチェック）
            if (otherSushi != null) otherSushi.SushiDestroy();
            SushiDestroy();
            return;
        }

        if (other.CompareTag("Customer"))
        {
            var customer = other.GetComponent<CustomerAI>();
            if (customer != null && customer.TryDeliver(data))
            {
                // 加算＋価格UIを表示
                ScoreManager.Instance?.AddScore(data.price);
                PricePopupManager.Instance?.ShowPopup(data.price, transform.position);
                SushiDestroy();
            }
        }
    }
}