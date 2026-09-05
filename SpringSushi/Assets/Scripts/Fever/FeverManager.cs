using System;
using UnityEngine;

/// <summary>
/// フィーバーフェーズの進行・状態を管理するSingleton。
/// </summary>
public class FeverManager : MonoBehaviour
{
    public static FeverManager Instance { get; private set; }

    [Header("フィーバー設定")]
    [Tooltip("フィーバーの継続時間（秒）")]
    [SerializeField] private float feverDuration = 15f;

    [Tooltip("フィーバー中の加点倍率")]
    [SerializeField] private float scoreMultiplier = 2.0f;

    public bool IsFever { get; private set; } = false;
    public float RemainingTime { get; private set; } = 0f;
    public float ScoreMultiplier => scoreMultiplier;

    public event Action OnFeverStarted;
    public event Action OnFeverEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!IsFever) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            EndFever();
        }
    }

    /// <summary>
    /// フィーバーフェーズを開始する
    /// </summary>
    public void StartFever()
    {
        IsFever = true;
        RemainingTime = feverDuration;
        Debug.Log("<color=magenta>【Fever】</color> フィーバーフェーズ突入！(15秒間)");

        OnFeverStarted?.Invoke();
    }

    /// <summary>
    /// フィーバーフェーズを終了し、デフォルトフェーズに戻る
    /// </summary>
    public void EndFever()
    {
        if (!IsFever) return;

        IsFever = false;
        RemainingTime = 0f;
        Debug.Log("<color=magenta>【Fever】</color> フィーバーフェーズ終了。デフォルトフェーズへ");

        OnFeverEnded?.Invoke();
    }

    /// <summary>
    /// スコア倍率を適用した加算額を取得（フィーバー中のみ適用）
    /// </summary>
    public int GetEarnedScore(int basePrice)
    {
        if (!IsFever) return basePrice;
        return Mathf.RoundToInt(basePrice * scoreMultiplier);
    }
}