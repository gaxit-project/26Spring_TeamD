using UnityEngine;

/// <summary>
/// フィーバー時の虹色エフェクト（FeverCanvas）とBGM切り替えを担当する。
/// </summary>
public class FeverEffectController : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("FeverCanvas内の虹色枠に付けたCanvasGroup")]
    [SerializeField] private CanvasGroup rainbowCanvasGroup;

    [Tooltip("アルファ値の補間速度")]
    [SerializeField] private float fadeSpeed = 5f;

    [Tooltip("虹色の縁取りが濃くなり始めるコンボ数（デフォルト: 5コンボ目から開始、8で全表示）")]
    [SerializeField] private int startFadeCombo = 5;

    [Header("虹色パルス演出（フィーバー中）")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseMinAlpha = 0.8f;

    private float targetAlpha = 0f;

    private void Start()
    {
        if (rainbowCanvasGroup != null)
        {
            rainbowCanvasGroup.alpha = 0f;
        }

        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboChanged += HandleComboChanged;

        if (FeverManager.Instance != null)
        {
            FeverManager.Instance.OnFeverStarted += HandleFeverStarted;
            FeverManager.Instance.OnFeverEnded += HandleFeverEnded;
        }
    }

    private void OnDestroy()
    {
        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboChanged -= HandleComboChanged;

        if (FeverManager.Instance != null)
        {
            FeverManager.Instance.OnFeverStarted -= HandleFeverStarted;
            FeverManager.Instance.OnFeverEnded -= HandleFeverEnded;
        }
    }

    private void Update()
    {
        if (rainbowCanvasGroup == null) return;

        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;

        if (isFever && enablePulse)
        {
            // フィーバー中はほんのり明滅
            float pulse = Mathf.Lerp(pulseMinAlpha, 1.0f, Mathf.PingPong(Time.time * pulseSpeed, 1f));
            rainbowCanvasGroup.alpha = Mathf.MoveTowards(rainbowCanvasGroup.alpha, pulse, fadeSpeed * Time.deltaTime);
        }
        else
        {
            rainbowCanvasGroup.alpha = Mathf.MoveTowards(rainbowCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// コンボ数に応じた虹色枠のアルファ値制御（startFadeCombo ? targetCount で徐々に濃く）
    /// </summary>
    private void HandleComboChanged(int currentCombo, int feverProgress, int targetCount)
    {
        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;
        if (isFever) return;

        if (feverProgress < startFadeCombo)
        {
            targetAlpha = 0f;
        }
        else
        {
            // 例: 5コンボで約0.33、6で0.66、7で0.85、8で1.0
            targetAlpha = Mathf.InverseLerp(startFadeCombo - 1, targetCount, feverProgress);
        }
    }

    private void HandleFeverStarted()
    {
        targetAlpha = 1.0f;

        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlayBGM(SoundKeys.BgmFever);
        }
    }

    private void HandleFeverEnded()
    {
        targetAlpha = 0f;

        if (GameStateManager.Instance == null || GameStateManager.Instance.IsPlaying)
        {
            if (SoundPlayer.Instance != null)
            {
                SoundPlayer.Instance.PlayBGM(SoundKeys.BgmGame);
            }
        }
    }
}