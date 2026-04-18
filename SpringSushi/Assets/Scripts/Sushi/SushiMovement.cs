using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    public float moveSpeed = 1.0f;

    public LaneSegment currentSegment;
    private float progress = 0f; // 0.0(nodeA) ～ 1.0(nodeB)

    public void Initialize(SushiData data, LaneSegment seg, LaneNode spawnNode)
    {
        this.data = data;
        currentSegment = seg;

        // ★ スポーンしたノードがAなら0から、Bなら1から開始
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

        // ★ 判定もシンプル：0を下回るか、1を上回ったら次のセグメントへ
        if (progress > 1.0f || progress < 0.0f)
        {
            SwitchToNextSegment();
        }
    }

    public void Move()
    {
        float totalDist = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);
        if (totalDist <= 0.001f) return;

        // ★ レーンの向きに従って増やすか減らすか決める（これだけで反転に対応）
        float moveDir = currentSegment.isReversed ? -1f : 1f;
        progress += (moveSpeed / totalDist) * Time.deltaTime * moveDir;

        // 描画用のt（0-1にクランプ）
        float t = Mathf.Clamp01(progress);

        // 位置の更新（常にAとBの間を補間）
        transform.position = Vector3.Lerp(currentSegment.nodeA.Position, currentSegment.nodeB.Position, t);

        // 向きの更新
        Vector3 forwardVec = currentSegment.nodeB.Position - currentSegment.nodeA.Position;
        if (forwardVec != Vector3.zero)
        {
            transform.forward = forwardVec * moveDir;
        }
    }

    // ★ 中身は不要になりました（Update内のMoveが自動でisReversedを見るため）
    public void SyncDirectionWithSegment() { }

    private void SwitchToNextSegment()
    {
        // どちらの端に到達したか
        LaneNode arrivalNode = (progress >= 0.5f) ? currentSegment.nodeB : currentSegment.nodeA;
        LaneSegment next = null;

        foreach (var seg in arrivalNode.connectedSegments)
        {
            if (seg == currentSegment) continue;
            next = seg;
            break;
        }

        if (next != null)
        {
            currentSegment = next;
            // ★ 次のセグメントのどちらのノードに入ったかでProgressをリセット
            progress = (arrivalNode == next.nodeA) ? 0f : 1f;
        }
        else
        {
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