using System;
using UnityEngine;

/// <summary>
/// 合計金額を管理するSingleton。
/// 加算：CustomerAIが寿司を受け取ったとき
/// 減算：寿司同士が衝突したとき
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int totalScore = 0;

    // ScoreUIが購読するイベント（変化量, 現在の合計）
    public event Action<int, int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーン遷移してもScoreManagerを破壊せず、ResultSceneへ引き継ぐ
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount)
    {
        totalScore += amount;
        OnScoreChanged?.Invoke(amount, totalScore);
        Debug.Log($"<color=green>[Score]</color> +{amount} → 合計: {totalScore}円");

        // ★ お金が増えた音を鳴らす（チャリンチャリンなど）
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.ScoreUp);
        }
    }

    public void SubtractScore(int amount)
    {
        totalScore -= amount;
        OnScoreChanged?.Invoke(-amount, totalScore);
        Debug.Log($"<color=red>[Score]</color> -{amount} → 合計: {totalScore}円");

        // ★ お金が減った音を鳴らす
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.ScoreDown);
        }
    }

    public void ResetScore()
    {
        totalScore = 0;
        OnScoreChanged?.Invoke(0, totalScore);
        Debug.Log("<color=cyan>[Score]</color> リセット");
    }

    public int TotalScore => totalScore;
}