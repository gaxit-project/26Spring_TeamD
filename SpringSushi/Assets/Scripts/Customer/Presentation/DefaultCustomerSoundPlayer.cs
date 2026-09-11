using UnityEngine;

/// <summary>
/// SoundPlayer.Instance への実際の橋渡し。本番環境ではこれを使う。
/// </summary>
public class DefaultCustomerSoundPlayer : ICustomerSoundPlayer
{
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

    // ★ 追加
    public void PlaySatisfiedVoice()
    {
        if (SoundPlayer.Instance == null) return;
        SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerSatisfied);
    }
}