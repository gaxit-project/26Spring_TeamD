using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent / NavMeshObstacle の切替と、座席への移動・到着判定を担当する。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerMovementController : MonoBehaviour
{
    private NavMeshAgent agent;
    private NavMeshObstacle navObstacle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();
        if (navObstacle != null)
        {
            navObstacle.enabled = false;
            navObstacle.carving = true;
        }
    }

    public void Bind(CustomerStateMachine stateMachine)
    {
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(CustomerAI.CustomerState newState)
    {
        switch (newState)
        {
            case CustomerAI.CustomerState.Seated:
            case CustomerAI.CustomerState.Ordering:
            case CustomerAI.CustomerState.Eating:
            case CustomerAI.CustomerState.Satisfied:
            case CustomerAI.CustomerState.Angry:
                agent.enabled = false;
                if (navObstacle != null) navObstacle.enabled = true;
                break;

            case CustomerAI.CustomerState.Leaving:
                if (navObstacle != null) navObstacle.enabled = false;
                agent.enabled = true;
                break;
        }
    }

    public void MoveToSeat(Transform seat) => agent.SetDestination(seat.position);

    /// <summary>毎フレーム呼ばれ、座席への到着を判定する。</summary>
    public bool HasArrivedAtSeat()
    {
        if (agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
    }
}