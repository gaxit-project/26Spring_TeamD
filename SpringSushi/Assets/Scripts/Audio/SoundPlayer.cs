using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SE・Voiceの単発再生とBGMのループ再生を担当するSingleton。
/// DontDestroyOnLoad。
///
/// 【使い方】
/// SE  : SoundPlayer.Instance.PlaySFX(SoundKeys.ButtonPress);
/// Voice: SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerSpawn);
/// BGM : SoundPlayer.Instance.PlayBGM(SoundKeys.BgmGame);
/// 停止 : SoundPlayer.Instance.StopBGM();
/// </summary>
public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance { get; private set; }

    [Header("データベース")]
    [SerializeField] private SoundDatabase database;

    [Header("BGM用AudioSource（ループ再生）")]
    [SerializeField] private AudioSource bgmSource;

    // 単発SE・Voice用のプール
    private readonly List<(AudioSource src, SoundCategory cat, float mul)> activeSources = new();
    private int lastUpdateCount = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (SoundManager.Instance == null) return;
        if (lastUpdateCount != SoundManager.Instance.VolumeUpdateCount)
        {
            lastUpdateCount = SoundManager.Instance.VolumeUpdateCount;
            RefreshAllVolumes();
            if (bgmSource != null)
                bgmSource.volume = SoundManager.Instance.GetVolume(SoundCategory.BGM);
        }
    }

    // -------------------------------------------------------
    // 公開API
    // -------------------------------------------------------

    /// <summary>SFXを鳴らす</summary>
    public void PlaySFX(string key) => PlayOneShot(key, SoundCategory.SFX);

    /// <summary>Voiceを鳴らす</summary>
    public void PlayVoice(string key) => PlayOneShot(key, SoundCategory.Voice);

    /// <summary>
    /// 寿司名ボイスを鳴らす。
    /// SoundDatabaseに "voice_sushi_{sushiName}" キーで登録する。
    /// </summary>
    public void PlaySushiVoice(string sushiName)
        => PlayVoice($"voice_sushi_{sushiName}");

    /// <summary>BGMを再生する（同じクリップなら再生しない）</summary>
    public void PlayBGM(string key)
    {
        if (database == null || bgmSource == null) return;
        var entry = database.Get(key);
        if (entry == null || entry.clip == null) return;
        if (bgmSource.clip == entry.clip && bgmSource.isPlaying) return;

        bgmSource.clip = entry.clip;
        bgmSource.loop = true;
        bgmSource.volume = SoundManager.Instance.GetVolume(SoundCategory.BGM) * entry.volumeMultiplier;
        bgmSource.Play();
    }

    /// <summary>BGMを停止する</summary>
    public void StopBGM()
    {
        bgmSource?.Stop();
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    private void PlayOneShot(string key, SoundCategory category)
    {
        if (database == null) return;
        var entry = database.Get(key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[SoundPlayer] キー '{key}' が見つかりません。SoundDatabaseを確認してください。");
            return;
        }

        // 一時的なGameObjectにAudioSourceを生成して再生
        var obj = new GameObject($"SE_{key}");
        DontDestroyOnLoad(obj);
        var src = obj.AddComponent<AudioSource>();
        src.clip = entry.clip;
        src.volume = SoundManager.Instance.GetVolume(category) * entry.volumeMultiplier;
        src.spatialBlend = 0f; // 2D再生
        src.Play();

        activeSources.Add((src, category, entry.volumeMultiplier));
        StartCoroutine(DestroyAfterPlay(obj, src, entry.clip.length));
    }

    private IEnumerator DestroyAfterPlay(GameObject obj, AudioSource src, float duration)
    {
        yield return new WaitForSeconds(duration);
        activeSources.RemoveAll(x => x.src == src);
        Destroy(obj);
    }

    private void RefreshAllVolumes()
    {
        activeSources.RemoveAll(x => x.src == null);
        foreach (var (src, cat, mul) in activeSources)
            src.volume = SoundManager.Instance.GetVolume(cat) * mul;
    }
}