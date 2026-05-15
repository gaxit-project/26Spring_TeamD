using UnityEngine;

/// <summary>
/// 音量管理のSingleton。DontDestroyOnLoad。
/// 実際の再生はSoundPlayerが担当する。
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("音量設定")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;

    /// <summary>音量変更のたびにインクリメント。PersistentAudioSourceが監視する。</summary>
    public int VolumeUpdateCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetVolume(SoundCategory category)
    {
        return category switch
        {
            SoundCategory.SFX => masterVolume * sfxVolume,
            SoundCategory.Voice => masterVolume * voiceVolume,
            SoundCategory.BGM => masterVolume * bgmVolume,
            _ => masterVolume,
        };
    }

    public void SetMasterVolume(float v) { masterVolume = v; VolumeUpdateCount++; }
    public void SetSfxVolume(float v) { sfxVolume = v; VolumeUpdateCount++; }
    public void SetVoiceVolume(float v) { voiceVolume = v; VolumeUpdateCount++; }
    public void SetBgmVolume(float v) { bgmVolume = v; VolumeUpdateCount++; }
}