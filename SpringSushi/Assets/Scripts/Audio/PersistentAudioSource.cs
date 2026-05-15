using UnityEngine;

/// <summary>
/// BGMなどループ再生するAudioSourceにアタッチして音量を自動更新する。
/// SoundPlayerのbgmSourceを使う場合は不要。
/// 独立したAudioSourceを使いたい場合に使う。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PersistentAudioSource : MonoBehaviour
{
    [SerializeField] private SoundCategory category = SoundCategory.BGM;
    private AudioSource audioSource;
    private int lastUpdateCount = -1;

    private void Awake() => audioSource = GetComponent<AudioSource>();

    private void Update()
    {
        if (SoundManager.Instance == null) return;
        if (lastUpdateCount == SoundManager.Instance.VolumeUpdateCount) return;
        lastUpdateCount = SoundManager.Instance.VolumeUpdateCount;
        audioSource.volume = SoundManager.Instance.GetVolume(category);
    }
}