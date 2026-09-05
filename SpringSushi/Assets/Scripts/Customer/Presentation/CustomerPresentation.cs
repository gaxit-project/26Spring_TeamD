using UnityEngine;

/// <summary>
/// 状態変化に応じたアニメーション反映とサウンド再生を担当する。
/// サウンド再生はICustomerSoundPlayer経由で行い、SoundPlayer.Instanceへの直接依存を避ける。
/// </summary>
public class CustomerPresentation : MonoBehaviour
{
    private CustomerAnimator customerAnimator;
    private ICustomerSoundPlayer soundPlayer = new DefaultCustomerSoundPlayer();

    public void SetVisual(CustomerAnimator animator, Transform seat)
    {
        customerAnimator = animator;
        customerAnimator?.SetSeat(seat);
    }

    public void SetSoundPlayer(ICustomerSoundPlayer player) => soundPlayer = player;

    public void Bind(CustomerStateMachine stateMachine)
    {
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    public void PlaySpawnVoice() => soundPlayer.PlaySpawnVoice();
    public void PlayAngryVoice() => soundPlayer.PlayAngryVoice();
    public void PlaySatisfiedVoice() => soundPlayer.PlaySatisfiedVoice(); // ★ 追加

    private void HandleStateChanged(CustomerAI.CustomerState newState)
    {
        customerAnimator?.ApplyState(newState);
        Debug.Log($"<color=cyan>[State]</color> {gameObject.name}: {newState}");
    }
}