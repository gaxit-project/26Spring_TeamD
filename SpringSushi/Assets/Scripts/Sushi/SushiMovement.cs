using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Settings")]
    public float moveSpeed = 1.0f;

    public LaneSegment currentSegment;
    private float progress = 0f;
    private bool movingTowardsNodeB = true;

    private SushiRegistry registry;

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
        {
            transform.forward = forwardVec * directionFactor;
        }
    }

    public void UpdateMovingDirection()
    {
        movingTowardsNodeB = !currentSegment.IsReversed;
    }

    public void SyncDirectionWithSegment()
    {
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