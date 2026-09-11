using System;
using UnityEngine;

/// <summary>
/// コンボ数とフィーバー突入用連続カウントを管理するSingleton。
/// </summary>
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("フィーバー突入設定")]
    [Tooltip("フィーバー突入に必要な連続成功回数（コンボ数）")]
    [SerializeField] private int targetFeverCount = 8; // ★ 8コンボに変更

    public int CurrentCombo { get; private set; } = 0;
    public int TargetFeverCount => targetFeverCount;

    // フィーバー突入用カウント（0?targetFeverCount）
    private int feverProgressCount = 0;

    // コンボキー配列
    private readonly string[] comboSoundKeys = new string[]
    {
        SoundKeys.Combo1, SoundKeys.Combo2, SoundKeys.Combo3, SoundKeys.Combo4, SoundKeys.Combo5,
        SoundKeys.Combo6, SoundKeys.Combo7, SoundKeys.Combo8, SoundKeys.Combo9, SoundKeys.Combo10
    };

    // UIやエフェクト連携用のイベント (現在の通算コンボ数, フィーバーまでの進捗, 目標数)
    public event Action<int, int, int> OnComboChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 寿司の提供成功時に呼ぶ
    /// </summary>
    public void IncrementCombo()
    {
        CurrentCombo++;

        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;

        if (isFever)
        {
            // フィーバー中は到達音（combo_8）を鳴らす
            PlayComboSound(targetFeverCount);
            if (SoundPlayer.Instance != null)
                SoundPlayer.Instance.PlaySFX(SoundKeys.Cheers);
            OnComboChanged?.Invoke(CurrentCombo, targetFeverCount, targetFeverCount);
        }
        else
        {
            feverProgressCount++;
            PlayComboSound(feverProgressCount);

            OnComboChanged?.Invoke(CurrentCombo, feverProgressCount, targetFeverCount);

            // ★ 8回連続成功でフィーバー突入（8個目の寿司からフィーバー倍率適用）
            if (feverProgressCount >= targetFeverCount)
            {
                if (SoundPlayer.Instance != null)
                {
                    SoundPlayer.Instance.PlaySFX(SoundKeys.Cheers);
                }
                feverProgressCount = 0; // フィーバー終了後の再カウント用にリセット
                FeverManager.Instance?.StartFever();
            }
        }
    }

    /// <summary>
    /// 失敗時（寿司衝突、客激怒退店）に呼ぶ
    /// </summary>
    public void ResetCombo()
    {
        CurrentCombo = 0;

        // フィーバー中でない場合は、フィーバー突入カウントもリセット
        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;
        if (!isFever)
        {
            feverProgressCount = 0;
        }

        OnComboChanged?.Invoke(CurrentCombo, feverProgressCount, targetFeverCount);
    }

    private void PlayComboSound(int count)
    {
        if (SoundPlayer.Instance == null) return;

        int index = Mathf.Clamp(count - 1, 0, comboSoundKeys.Length - 1);
        SoundPlayer.Instance.PlaySFX(comboSoundKeys[index]);
    }
}