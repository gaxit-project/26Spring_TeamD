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
    private Transform seatTransform;

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

    /// <summary>
    /// 着席する座席(SeatAnchor)を設定する。着席時の向きは、
    /// 自前でレーンを探して計算するのではなく、SeatBuilderが既に正しく計算・設定した
    /// 実際の椅子モデル(anchorの子オブジェクト)の向きをそのまま採用する。
    /// これにより、椅子の向き計算ロジックが1箇所(SeatBuilder)に一本化され、ズレが起きない。
    /// </summary>
    public void SetSeat(Transform seat) => seatTransform = seat;

    private void FaceSeat()
    {
        if (seatTransform == null) return;

        // SeatBuilderが生成した椅子(anchorの最初の子)の向きをそのまま使う
        Transform chair = seatTransform.childCount > 0 ? seatTransform.GetChild(0) : seatTransform;
        transform.rotation = chair.rotation * Quaternion.Euler(0f, -90f, 0f);
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
                FaceSeat();
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

            case CustomerAI.CustomerState.Angry:
                anim.SetBool(HashIsSeated, true);
                anim.SetBool(HashIsAngry, true);
                if (angryParticle != null) angryParticle.Play();
                break;

            case CustomerAI.CustomerState.Leaving:
                anim.SetBool(HashIsLeaving, true);
                if (angryParticle != null) angryParticle.Stop();
                break;
        }
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