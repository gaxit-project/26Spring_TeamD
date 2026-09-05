using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    [Tooltip("通常時の移動速度")]
    [SerializeField] private float normalSpeed = 4.0f; // ★ 通常速度 4

    [Tooltip("フィーバー時の移動速度")]
    [SerializeField] private float feverSpeed = 6.0f;  // ★ フィーバー時速度 6

    /// <summary>
    /// 現在適用される移動速度（フィーバー中なら自動でfeverSpeedになる）
    /// </summary>
    public float CurrentMoveSpeed =>
        (FeverManager.Instance != null && FeverManager.Instance.IsFever) ? feverSpeed : normalSpeed;

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

    // 衝突処理済みフラグ（二重発火防止）
    private bool isCollisionHandled = false;

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

        // ★ moveSpeed の代わりに CurrentMoveSpeed (4 または 6) を使用
        progress += (CurrentMoveSpeed / totalDist) * Time.deltaTime * directionFactor;
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

    public void TryExitStuck()
    {
        if (!isStuck) return;

        LaneSegment exitSeg = stuckNode.GetExitSegment();
        if (exitSeg == null) return;

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

    private void OnTriggerStay(Collider other)
    {
        // ----------------------------------------------------
        // 寿司同士の衝突
        // ----------------------------------------------------
        if (other.CompareTag("Sushi"))
        {
            var otherSushi = other.GetComponent<SushiMovement>();
            if (otherSushi == null) return;

            if (isCollisionHandled || otherSushi.isCollisionHandled) return;
            if (gameObject.GetInstanceID() < otherSushi.gameObject.GetInstanceID()) return;

            isCollisionHandled = true;
            otherSushi.isCollisionHandled = true;

            bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;

            // ★ フィーバー中でない場合のみ、スコア減算と赤字ポップアップを行う
            // （SubtractScore を呼ばないため、ScoreDownの減算SEも鳴りません）
            if (!isFever)
            {
                int lossAmount = data.price + otherSushi.data.price;
                ScoreManager.Instance?.SubtractScore(lossAmount);

                Vector3 popupPos = (transform.position + otherSushi.transform.position) * 0.5f;
                PricePopupManager.Instance?.ShowPopup(-lossAmount, popupPos);
            }

            // 失敗によるコンボリセット（フィーバー中もコンボ数は途切れる仕様のため呼び出し）
            ComboManager.Instance?.ResetCombo();

            // 皿の割れる音
            if (SoundPlayer.Instance != null)
                SoundPlayer.Instance.PlaySFX(SoundKeys.PlateBreak);

            otherSushi.SushiDestroy();
            SushiDestroy();
            return;
        }

        // ----------------------------------------------------
        // お客さんへの提供
        // ----------------------------------------------------
        if (other.CompareTag("Customer"))
        {
            if (isCollisionHandled) return;

            var customer = other.GetComponent<CustomerAI>();
            if (customer != null && customer.TryDeliver(data))
            {
                isCollisionHandled = true;

                // 先にコンボを加算（8コンボ目ならここでフィーバーへ突入）
                ComboManager.Instance?.IncrementCombo();

                // フィーバー倍率を適用したスコアを加算
                int earnedScore = FeverManager.Instance != null
                    ? FeverManager.Instance.GetEarnedScore(data.price)
                    : data.price;

                ScoreManager.Instance?.AddScore(earnedScore);
                PricePopupManager.Instance?.ShowPopup(earnedScore, transform.position);

                SushiDestroy();
            }
        }
    }
}