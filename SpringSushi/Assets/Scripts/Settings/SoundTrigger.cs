using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] private SoundDatabase database;

    public void PlayByKey(string key, Vector3 position)
    {
        if (database == null) return;

        var sound = database.GetSound(key);
        if (sound == null) return;

        // ★修正: データベースに設定された volumeMultiplier を渡す
        InstantAudioSource.Instance.PlaySound(
            sound.clip,
            sound.category,
            position,
            sound.volumeMultiplier
        );
    }
}