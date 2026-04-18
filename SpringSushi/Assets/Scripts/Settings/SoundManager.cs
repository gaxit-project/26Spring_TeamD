using UnityEngine;
using System; // Actionを使うために必要

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float environmentVolume = 0.8f;

    // イベントの代わりに「音量更新カウンター」を作る
    public int VolumeUpdateCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float value)
    {
        // あまりに急激な変化（起動直後の0へのリセットなど）をログで追えるようにする
        masterVolume = value;
        VolumeUpdateCount++;
        Debug.Log($"SoundManager: MasterVolumeが {value} にセットされました");
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = value;
        VolumeUpdateCount++;

        // ★ここが重要！誰がこのメソッドを呼んだか特定します
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        string callerName = stackTrace.GetFrame(1).GetMethod().DeclaringType.Name;
        Debug.Log($"<color=red>【音量変更検知】</color> {callerName} が BGM音量を {value} にしました");
    }
    public void SetEnvVolume(float value)
    {
        environmentVolume = value;
        VolumeUpdateCount++;
    }

    public float GetVolume(SoundCategory category)
    {
        float baseVolume = masterVolume;
        if (category == SoundCategory.BGM) baseVolume *= bgmVolume;
        else baseVolume *= environmentVolume;
        return baseVolume;
    }
}