using UnityEngine;
using System.Collections.Generic;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance { get; private set; }

    [Header("データベース（未アサインなら Resources/SoundDatabase を自動ロード）")]
    [SerializeField] private SoundDatabase database;

    [Header("BGM用AudioSource（ループ再生）")]
    [SerializeField] private AudioSource bgmSource;

    [Header("オブジェクトプール設定")]
    [SerializeField] private int initialPoolSize = 15;

    private class PooledSource
    {
        public AudioSource Source;
        public SoundCategory Category;
        public float VolumeMultiplier;
    }

    private readonly List<PooledSource> sfxPool = new();
    private int lastUpdateCount = -1;
    private float currentBgmMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (database == null)
        {
            database = Resources.Load<SoundDatabase>("SoundDatabase");
            if (database == null)
                Debug.LogError("[SoundPlayer] SoundDatabase が見つかりません。");
            else
                Debug.Log("[SoundPlayer] SoundDatabase 自動ロード成功。");
        }
        else
        {
            Debug.Log("[SoundPlayer] SoundDatabase はInspectorからアサイン済み。");
        }

        if (bgmSource == null)
            Debug.LogError("[SoundPlayer] bgmSource がアサインされていません。");

        InitializePool();
        Debug.Log($"[SoundPlayer] 初期化完了。プールサイズ={sfxPool.Count}");
    }

    private void Update()
    {
        // ★ SoundManager が null でも例外を出さない
        if (SoundManager.Instance == null) return;
        if (lastUpdateCount == SoundManager.Instance.VolumeUpdateCount) return;
        lastUpdateCount = SoundManager.Instance.VolumeUpdateCount;
        RefreshAllVolumes();
    }

    // -------------------------------------------------------
    // プール管理
    // -------------------------------------------------------

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
            CreatePooledSource();
    }

    private PooledSource CreatePooledSource()
    {
        var obj = new GameObject($"SE_Source_{sfxPool.Count}");
        obj.transform.SetParent(transform);
        var src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        var pooled = new PooledSource { Source = src };
        sfxPool.Add(pooled);
        return pooled;
    }

    private PooledSource GetAvailableSource()
    {
        foreach (var pooled in sfxPool)
            if (!pooled.Source.isPlaying) return pooled;
        Debug.LogWarning($"[SoundPlayer] プール枯渇。追加生成。サイズ:{sfxPool.Count + 1}");
        return CreatePooledSource();
    }

    // -------------------------------------------------------
    // 公開API
    // -------------------------------------------------------

    public void PlaySFX(string key) => PlayOneShot(key);
    public void PlayVoice(string key) => PlayOneShot(key);
    public void PlaySushiVoice(string sushiName) => PlayOneShot($"voice_sushi_{sushiName}");

    public void PlayBGM(string key)
    {
        if (database == null) { Debug.LogError("[SoundPlayer] PlayBGM: database null"); return; }
        if (bgmSource == null) { Debug.LogError("[SoundPlayer] PlayBGM: bgmSource null"); return; }

        var entry = database.Get(key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[SoundPlayer] PlayBGM: キー '{key}' が見つかりません。");
            return;
        }
        if (bgmSource.clip == entry.clip && bgmSource.isPlaying) return;

        currentBgmMultiplier = entry.volumeMultiplier;
        bgmSource.clip = entry.clip;
        bgmSource.loop = true;

        // ★ SoundManager が null でも音量1で再生を試みる
        float vol = SoundManager.Instance != null
            ? SoundManager.Instance.GetVolume(SoundCategory.BGM) * currentBgmMultiplier
            : currentBgmMultiplier;

        bgmSource.volume = vol;
        bgmSource.Play();
        Debug.Log($"[SoundPlayer] PlayBGM: '{key}' 再生開始。vol={vol}");
    }

    public void StopBGM()
    {
        bgmSource?.Stop();
        Debug.Log("[SoundPlayer] StopBGM 呼び出し。");
    }

    // -------------------------------------------------------
    // 内部処理
    // -------------------------------------------------------

    private void PlayOneShot(string key)
    {
        if (database == null) { Debug.LogError("[SoundPlayer] PlayOneShot: database null"); return; }

        var entry = database.Get(key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[SoundPlayer] キー '{key}' が見つかりません。");
            return;
        }

        var pooled = GetAvailableSource();
        pooled.Category = entry.category;
        pooled.VolumeMultiplier = entry.volumeMultiplier;
        pooled.Source.clip = entry.clip;

        // ★ SoundManager が null でも音量1で再生を試みる
        float vol = SoundManager.Instance != null
            ? SoundManager.Instance.GetVolume(entry.category) * entry.volumeMultiplier
            : entry.volumeMultiplier;

        pooled.Source.volume = vol;
        pooled.Source.Play();
    }

    private void RefreshAllVolumes()
    {
        if (SoundManager.Instance == null) return;

        if (bgmSource != null)
            bgmSource.volume = SoundManager.Instance.GetVolume(SoundCategory.BGM) * currentBgmMultiplier;

        foreach (var pooled in sfxPool)
        {
            if (pooled.Source.isPlaying)
                pooled.Source.volume = SoundManager.Instance.GetVolume(pooled.Category) * pooled.VolumeMultiplier;
        }
    }
}