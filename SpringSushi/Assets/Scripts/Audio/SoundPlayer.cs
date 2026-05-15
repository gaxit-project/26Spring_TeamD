using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SE・Voiceの単発再生とBGMのループ再生を担当するSingleton。
/// DontDestroyOnLoad。
/// オブジェクトプールを使用して、再生時のメモリ負荷（GCスパイク）をゼロにしています。
/// </summary>
public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance { get; private set; }

    [Header("データベース")]
    [SerializeField] private SoundDatabase database;

    [Header("BGM用AudioSource（ループ再生）")]
    [SerializeField] private AudioSource bgmSource;

    [Header("オブジェクトプール設定")]
    [Tooltip("起動時に生成するAudioSourceの数")]
    [SerializeField] private int initialPoolSize = 15;

    // プールされたAudioSourceと、その現在の設定を紐づけて管理するクラス
    private class PooledSource
    {
        public AudioSource Source;
        public SoundCategory Category;
        public float VolumeMultiplier;
    }

    // 単発SE・Voice用のプール
    private readonly List<PooledSource> sfxPool = new();
    private int lastUpdateCount = -1;
    private float currentBgmMultiplier = 1f; // BGMの個別音量倍率を保持

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
    }

    private void Update()
    {
        if (SoundManager.Instance == null) return;
        if (lastUpdateCount != SoundManager.Instance.VolumeUpdateCount)
        {
            lastUpdateCount = SoundManager.Instance.VolumeUpdateCount;
            RefreshAllVolumes();
        }
    }

    // -------------------------------------------------------
    // プール管理
    // -------------------------------------------------------

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledSource();
        }
    }

    private PooledSource CreatePooledSource()
    {
        // SoundPlayerの子オブジェクトとして生成
        var obj = new GameObject($"SE_Source_{sfxPool.Count}");
        obj.transform.SetParent(transform);

        var src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D再生

        var pooled = new PooledSource { Source = src };
        sfxPool.Add(pooled);
        return pooled;
    }

    private PooledSource GetAvailableSource()
    {
        // 1. 再生されていない（空いている）ソースを探す
        foreach (var pooled in sfxPool)
        {
            if (!pooled.Source.isPlaying) return pooled;
        }

        // 2. 全て使用中の場合は、プールを動的に拡張する
        Debug.LogWarning($"[SoundPlayer] プールが枯渇したため AudioSource を追加生成します。現在のサイズ: {sfxPool.Count + 1}");
        return CreatePooledSource();
    }

    // -------------------------------------------------------
    // 公開API
    // -------------------------------------------------------

    /// <summary>SFXを鳴らす</summary>
    public void PlaySFX(string key) => PlayOneShot(key);

    /// <summary>Voiceを鳴らす</summary>
    public void PlayVoice(string key) => PlayOneShot(key);

    /// <summary>
    /// 寿司名ボイスを鳴らす。
    /// SoundDatabaseに "voice_sushi_{sushiName}" キーで登録する。
    /// </summary>
    public void PlaySushiVoice(string sushiName)
        => PlayOneShot($"voice_sushi_{sushiName}");

    /// <summary>BGMを再生する（同じクリップなら再生しない）</summary>
    public void PlayBGM(string key)
    {
        if (database == null || bgmSource == null) return;
        var entry = database.Get(key);
        if (entry == null || entry.clip == null) return;
        if (bgmSource.clip == entry.clip && bgmSource.isPlaying) return;

        currentBgmMultiplier = entry.volumeMultiplier; // 音量更新用に倍率を保持

        bgmSource.clip = entry.clip;
        bgmSource.loop = true;
        bgmSource.volume = SoundManager.Instance.GetVolume(SoundCategory.BGM) * currentBgmMultiplier;
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

    private void PlayOneShot(string key)
    {
        if (database == null) return;
        var entry = database.Get(key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[SoundPlayer] キー '{key}' が見つかりません。SoundDatabaseを確認してください。");
            return;
        }

        // プールから空いているAudioSourceを取得
        var pooled = GetAvailableSource();

        // カテゴリや倍率の設定を更新
        pooled.Category = entry.category;
        pooled.VolumeMultiplier = entry.volumeMultiplier;

        // 再生
        pooled.Source.clip = entry.clip;
        pooled.Source.volume = SoundManager.Instance.GetVolume(entry.category) * entry.volumeMultiplier;
        pooled.Source.Play();
    }

    private void RefreshAllVolumes()
    {
        // BGMの音量更新
        if (bgmSource != null)
        {
            bgmSource.volume = SoundManager.Instance.GetVolume(SoundCategory.BGM) * currentBgmMultiplier;
        }

        // SE/Voiceの音量更新（再生中のもののみ）
        foreach (var pooled in sfxPool)
        {
            if (pooled.Source.isPlaying)
            {
                pooled.Source.volume = SoundManager.Instance.GetVolume(pooled.Category) * pooled.VolumeMultiplier;
            }
        }
    }
}