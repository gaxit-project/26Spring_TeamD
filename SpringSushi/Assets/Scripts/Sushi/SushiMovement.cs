using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    public float moveSpeed = 1.0f;

    public LaneSegment currentSegment;
    private LaneNode startNode;
    private LaneNode targetNode;

    private float progress = 0f;
    private int lastReverseFrame = -1; // ★多重反転防止用

    public void Initialize(SushiData data, LaneSegment seg, LaneNode spawnNode)
    {
        this.data = data;
        currentSegment = seg;
        startNode = spawnNode;
        targetNode = (spawnNode == seg.nodeA) ? seg.nodeB : seg.nodeA;
        progress = 0f;

        if (data?.sushiModel != null)
        {
            GameObject model = Instantiate(data.sushiModel, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (currentSegment == null || startNode == null || targetNode == null) return;

        Move();

        if (progress >= 1.0f)
        {
            SwitchToNextSegment();
        }
    }

    private void Move()
    {
        float totalDist = Vector3.Distance(startNode.Position, targetNode.Position);

        if (totalDist > 0.001f)
        {
            progress += (moveSpeed / totalDist) * Time.deltaTime;
        }

        float t = Mathf.Clamp01(progress);

        // 位置の更新
        transform.position = Vector3.Lerp(startNode.Position, targetNode.Position, t);

        // 向きの更新
        Vector3 dir = targetNode.Position - startNode.Position;
        if (dir != Vector3.zero) transform.forward = dir;
    }

    public void ToggleDirection()
    {
        // ★ 同じフレームで2回以上呼ばれたら無視する（反転の打ち消し防止）
        if (Time.frameCount == lastReverseFrame) return;
        lastReverseFrame = Time.frameCount;

        if (startNode == null || targetNode == null) return;

        // ノードの入れ替え
        LaneNode temp = startNode;
        startNode = targetNode;
        targetNode = temp;

        progress = 1.0f - progress;

        Debug.Log($"<color=orange>[Reverse]</color> {gameObject.name} : 方向転換完了 (Frame: {Time.frameCount})");
    }

    private void SwitchToNextSegment()
    {
        LaneNode arrivalNode = targetNode;
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
            startNode = arrivalNode;

            // 次のレーンの設定に従って目的地を決定
            targetNode = next.isReversed ? next.GetEntryNode() : next.GetExitNode();

            if (targetNode == startNode)
            {
                targetNode = (next.nodeA == startNode) ? next.nodeB : next.nodeA;
            }

            progress = 0f;
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
            Debug.Log($"<color=red>衝突消滅!</color> {data?.sushiName} が衝突");
            SushiDestroy();
        }
    }
}