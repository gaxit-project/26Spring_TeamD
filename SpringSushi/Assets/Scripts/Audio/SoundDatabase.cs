using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全シーン共通のサウンド定義。
/// SoundManagerにアサインして使う。
/// キーの一覧はSoundKeys.csを参照。
/// </summary>
[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("SoundKeys.csで定義したキーと一致させる")]
        public string key;
        public AudioClip clip;
        public SoundCategory category = SoundCategory.SFX;
        [Range(0.05f, 3f)] public float volumeMultiplier = 1f;
    }

    public List<SoundEntry> sounds = new();
    private Dictionary<string, SoundEntry> dict;

    private void OnEnable() => BuildDict();
    private void OnValidate() => BuildDict();

    private void BuildDict()
    {
        dict = new Dictionary<string, SoundEntry>();
        foreach (var s in sounds)
        {
            if (s == null || string.IsNullOrEmpty(s.key)) continue;
            if (!dict.ContainsKey(s.key)) dict[s.key] = s;
        }
    }

    public SoundEntry Get(string key)
    {
        if (dict == null || dict.Count != sounds.Count) BuildDict();
        return dict.TryGetValue(key, out var e) ? e : null;
    }
}