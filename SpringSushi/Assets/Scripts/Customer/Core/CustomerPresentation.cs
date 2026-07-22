using UnityEngine;

/// <summary>
/// 状態変化に応じたアニメーション反映とサウンド再生を担当する。
/// </summary>
public class CustomerPresentation : MonoBehaviour
{
    private CustomerAnimator customerAnimator;

    public void SetVisual(CustomerAnimator animator) => customerAnimator = animator;

    public void Bind(CustomerStateMachine stateMachine)
    {
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    public void PlaySpawnVoice()
    {
        if (SoundPlayer.Instance == null) return;
        SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerSpawn);
        SoundPlayer.Instance.PlayVoice(SoundKeys.EntryVoice);
    }

    public void PlayAngryVoice()
    {
        if (SoundPlayer.Instance == null) return;
        var key = Random.Range(0, 2) == 0 ? SoundKeys.CustomerAngry : SoundKeys.CustomerAngry2;
        SoundPlayer.Instance.PlayVoice(key);
    }

    private void HandleStateChanged(CustomerAI.CustomerState newState)
    {
        customerAnimator?.ApplyState(newState);
        Debug.Log($"<color=cyan>[State]</color> {gameObject.name}: {newState}");
    }
}