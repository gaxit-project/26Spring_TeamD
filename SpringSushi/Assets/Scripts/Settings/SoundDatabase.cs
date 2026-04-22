using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public string key;
        public AudioClip clip;
        public SoundCategory category = SoundCategory.SFX;
        [Range(0.1f, 10f)] public float volumeMultiplier = 1f; // 20倍までスライダーで調整可能に
    }

    public List<SoundEntry> sounds = new List<SoundEntry>();
    private Dictionary<string, SoundEntry> soundDict;

    // インスペクターで値をいじった時に即座に反映させる
    private void OnValidate()
    {
        InitializeDictionary();
    }

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDict = new Dictionary<string, SoundEntry>();
        foreach (var s in sounds)
        {
            if (s != null && !string.IsNullOrEmpty(s.key))
            {
                if (!soundDict.ContainsKey(s.key))
                    soundDict.Add(s.key, s);
            }
        }
    }

    public SoundEntry GetSound(string key)
    {
        // 辞書が空、またはエディタ実行中に中身がズレた場合を想定
        if (soundDict == null || soundDict.Count != sounds.Count)
        {
            InitializeDictionary();
        }

        if (soundDict.TryGetValue(key, out SoundEntry entry))
        {
            return entry;
        }

        return null;
    }
}