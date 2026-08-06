using UnityEngine;

/// <summary>
/// 状態変化に応じたアニメーション反映とサウンド再生を担当する。
/// サウンド再生はICustomerSoundPlayer経由で行い、SoundPlayer.Instanceへの直接依存を避ける。
/// </summary>
public class CustomerPresentation : MonoBehaviour
{
    private CustomerAnimator customerAnimator;
    private ICustomerSoundPlayer soundPlayer = new DefaultCustomerSoundPlayer();

    /// <summary>
    /// 見た目(Animator)と、着席する座席を設定する。
    /// 座席の情報はCustomerAnimator.SetSeatへそのまま渡し、着席時の向き計算に使わせる。
    /// </summary>
    public void SetVisual(CustomerAnimator animator, Transform seat)
    {
        customerAnimator = animator;
        customerAnimator?.SetSeat(seat);
    }

    /// <summary>テストや演出差し替え時に、サウンド再生の実装を差し替える。</summary>
    public void SetSoundPlayer(ICustomerSoundPlayer player) => soundPlayer = player;

    public void Bind(CustomerStateMachine stateMachine)
    {
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    public void PlaySpawnVoice() => soundPlayer.PlaySpawnVoice();
    public void PlayAngryVoice() => soundPlayer.PlayAngryVoice();

    private void HandleStateChanged(CustomerAI.CustomerState newState)
    {
        customerAnimator?.ApplyState(newState);
        Debug.Log($"<color=cyan>[State]</color> {gameObject.name}: {newState}");
    }
}