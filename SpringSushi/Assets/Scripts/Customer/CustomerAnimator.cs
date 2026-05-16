using UnityEngine;

/// <summary>
/// CustomerAI の State を Animator に橋渡しする。
/// customerPrefab のルートにアタッチし、
/// CustomerAI 側から OnStateChanged() を呼ぶだけでよい。
/// </summary>
[RequireComponent(typeof(Animator))]
public class CustomerAnimator : MonoBehaviour
{
    // AnimatorController のパラメータ名と必ず一致させること
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsSeated = Animator.StringToHash("IsSeated");
    private static readonly int HashIsOrdering = Animator.StringToHash("IsOrdering");
    private static readonly int HashIsAngry = Animator.StringToHash("IsAngry");
    private static readonly int HashIsLeaving = Animator.StringToHash("IsLeaving");
    private static readonly int HashEat = Animator.StringToHash("Eat");
    private static readonly int HashSatisfied = Animator.StringToHash("Satisfied");

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// CustomerAI.SetState() から呼ぶ。
    /// </summary>
    public void ApplyState(CustomerAI.CustomerState state)
    {
        // すべてのboolをリセットしてから該当フラグを立てる
        // → 遷移条件がシンプルになり、競合しない
        ResetAllBools();

        switch (state)
        {
            case CustomerAI.CustomerState.Walking:
                anim.SetBool(HashIsMoving, true);
                break;

            case CustomerAI.CustomerState.Seated:
                anim.SetBool(HashIsSeated, true);
                break;

            case CustomerAI.CustomerState.Ordering:
                anim.SetBool(HashIsSeated, true);
                anim.SetBool(HashIsOrdering, true);
                break;

            case CustomerAI.CustomerState.Eating:
                // Eating は一発トリガーで再生し、終わったらSeatedへ戻す
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

    // PatienceがなくなりAngry状態になったことを外部から通知する用
    public void PlayAngry()
    {
        ResetAllBools();
        anim.SetBool(HashIsAngry, true);
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