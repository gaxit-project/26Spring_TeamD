using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ボタンにアタッチするだけで、フォーカス/ホバー時のカーソル音と、
/// クリック/決定時の音を鳴らすスクリプト。
/// 
/// ■ 多重発火対策
///   1. フレームデバウンス : OnPointerClick と OnSubmit が同一フレームで
///      両方飛んできても ButtonPress は1回しか鳴らない。
///   2. Lock() : 各UIクラスがボタンを確定したタイミングで呼ぶと、
///      以降の ButtonPress を完全に封鎖する（音も鳴らない）。
/// </summary>
public class UIButtonSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    // ==========================================
    // 内部状態
    // ==========================================

    // ButtonPress を鳴らしたフレーム番号（同一フレーム内の二重発火を防ぐ）
    private int lastPressFrame = -1;

    // Lock() 後は ButtonPress を一切鳴らさない
    private bool isLocked = false;

    // ==========================================
    // 外部API
    // ==========================================

    /// <summary>
    /// ボタンが確定したことを通知する。
    /// 以降の ButtonPress 音を封鎖します。
    /// 各UIクラスの isButtonHandled = true と同じタイミングで呼んでください。
    /// </summary>
    public void Lock()
    {
        isLocked = true;
    }

    // ==========================================
    // カーソル移動音（選択・ホバー）
    // ==========================================

    public void OnSelect(BaseEventData eventData)
    {
        PlaySound(SoundKeys.CursorMove);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(SoundKeys.CursorMove);
    }

    // ==========================================
    // 決定音（クリック・エンターキー）
    // ==========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        TryPlayPressSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        TryPlayPressSound();
    }

    // ==========================================
    // 内部処理
    // ==========================================

    /// <summary>
    /// ButtonPress を「1ボタン操作につき1回」だけ鳴らす。
    /// - isLocked : 確定済みボタンは無条件スキップ
    /// - lastPressFrame : 同一フレーム内の OnPointerClick + OnSubmit 二重発火を防ぐ
    /// </summary>
    private void TryPlayPressSound()
    {
        if (isLocked) return;
        if (Time.frameCount == lastPressFrame) return;

        lastPressFrame = Time.frameCount;
        PlaySound(SoundKeys.ButtonPress);
    }

    private void PlaySound(string soundKey)
    {
        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.PlaySFX(soundKey);
    }
}