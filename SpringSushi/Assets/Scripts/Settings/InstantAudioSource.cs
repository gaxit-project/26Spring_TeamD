using UnityEngine;
using System.Collections.Generic;

public class InstantAudioSource : MonoBehaviour
{
    public static InstantAudioSource Instance { get; private set; }

    [SerializeField] private GameObject audioSourcePrefab;

    private List<(AudioSource source, SoundCategory category, float multiplier)> activeSources =
        new List<(AudioSource, SoundCategory, float)>();

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
            UpdateAllActiveVolumes();
        }
    }

    public void PlaySound(AudioClip clip, SoundCategory category, Vector3 position, float multiplier = 1f)
    {
        if (clip == null) return;

        GameObject obj = Instantiate(audioSourcePrefab, position, Quaternion.identity);
        AudioSource source = obj.GetComponent<AudioSource>();

        float baseVolume = SoundManager.Instance.GetVolume(category);
        float finalVolume = baseVolume * multiplier;

        source.clip = clip;
        source.volume = finalVolume;
        source.spatialBlend = 1f; // 3D音響
        source.spatialBlend = 0f; // 1f から 0f に一時的に変更
        source.Play();

        // デバッグログ：音量の内訳を表示
        Debug.Log($"[SE再生] {clip.name} | カテゴリ音量:{baseVolume:F2} | 倍率:{multiplier:F2} | 最終:{finalVolume:F2}");

        activeSources.Add((source, category, multiplier));
        StartCoroutine(DestroyAndRemove(obj, source, clip.length));
    }

    private System.Collections.IEnumerator DestroyAndRemove(GameObject obj, AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeSources.RemoveAll(x => x.source == source);
        Destroy(obj);
    }

    private void UpdateAllActiveVolumes()
    {
        activeSources.RemoveAll(x => x.source == null);

        foreach (var item in activeSources)
        {
            if (item.source != null)
            {
                item.source.volume = SoundManager.Instance.GetVolume(item.category) * item.multiplier;
            }
        }
    }
}