using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CustomerAnimator : MonoBehaviour
{
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsSeated = Animator.StringToHash("IsSeated");
    private static readonly int HashIsOrdering = Animator.StringToHash("IsOrdering");
    private static readonly int HashIsAngry = Animator.StringToHash("IsAngry");
    private static readonly int HashIsLeaving = Animator.StringToHash("IsLeaving");
    private static readonly int HashEat = Animator.StringToHash("Eat");
    private static readonly int HashSatisfied = Animator.StringToHash("Satisfied");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");

    [Header("怒りエフェクト")]
    [SerializeField] private ParticleSystem angryParticle;

    private Animator anim;
    private NavMeshAgent agent;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent != null)
            anim.SetFloat(HashSpeed, agent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    // ─────────────────────────────────────
    // ★ 着席時に最寄りLaneSegmentへ向きを合わせる
    // CustomerAI.SetState(Seated) のタイミングで呼ぶ
    // ─────────────────────────────────────
    public void FaceNearestLane()
    {
        var segments = Object.FindObjectsByType<LaneSegment>(FindObjectsSortMode.None);
        Quaternion rot = CalcFacingRotation(transform.position, segments);
        transform.rotation = rot;
    }

    /// <summary>
    /// SeatBuilderのCalcSeatRotationと同じロジック。
    /// 最寄りLaneSegmentのA→B方向にZ+を向け、180°反転して客席向きにする。
    /// </summary>
    private Quaternion CalcFacingRotation(Vector3 pos, LaneSegment[] segments)
    {
        if (segments == null || segments.Length == 0) return Quaternion.identity;

        LaneSegment nearest = null;
        float minDist = float.MaxValue;

        foreach (var seg in segments)
        {
            if (seg.nodeA == null || seg.nodeB == null) continue;
            Vector3 mid = (seg.nodeA.Position + seg.nodeB.Position) * 0.5f;
            float dist = Vector3.Distance(pos, mid);
            if (dist < minDist) { minDist = dist; nearest = seg; }
        }

        if (nearest == null) return Quaternion.identity;

        Vector3 segDir = (nearest.nodeB.Position - nearest.nodeA.Position).normalized;
        if (segDir == Vector3.zero) return Quaternion.identity;

        return Quaternion.LookRotation(segDir, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
    }

    public void ApplyState(CustomerAI.CustomerState state)
    {
        ResetAllBools();
        switch (state)
        {
            case CustomerAI.CustomerState.Walking:
                anim.SetBool(HashIsMoving, true);
                break;

            case CustomerAI.CustomerState.Seated:
                anim.SetBool(HashIsSeated, true);
                FaceNearestLane(); // ★ 着席と同時に向きを確定
                break;

            case CustomerAI.CustomerState.Ordering:
                anim.SetBool(HashIsSeated, true);
                anim.SetBool(HashIsOrdering, true);
                break;

            case CustomerAI.CustomerState.Eating:
                anim.SetBool(HashIsSeated, true);
                anim.SetTrigger(HashEat);
                break;

            case CustomerAI.CustomerState.Satisfied:
                anim.SetBool(HashIsSeated, true);
                anim.SetTrigger(HashSatisfied);
                break;

            case CustomerAI.CustomerState.Leaving:
                anim.SetBool(HashIsLeaving, true);
                break;
        }
    }

    public void PlayAngry()
    {
        ResetAllBools();
        anim.SetBool(HashIsAngry, true);

        // ★ 怒りParticleを再生
        if (angryParticle != null)
            angryParticle.Play();
    }

    private void ResetAllBools()
    {
        anim.SetBool(HashIsMoving, false);
        anim.SetBool(HashIsSeated, false);
        anim.SetBool(HashIsOrdering, false);
        anim.SetBool(HashIsAngry, false);
        anim.SetBool(HashIsLeaving, false);
    }
}