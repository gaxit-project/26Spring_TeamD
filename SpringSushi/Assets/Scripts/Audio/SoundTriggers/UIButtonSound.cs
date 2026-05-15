using UnityEngine;
using UnityEngine.EventSystems; // UIのイベントを扱うために必要

/// <summary>
/// ボタンにアタッチするだけで、フォーカス/ホバー時のカーソル音と、クリック/決定時の音を鳴らすスクリプト
/// </summary>
public class UIButtonSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    // ==========================================
    // カーソル移動音（選択・ホバー）
    // ==========================================

    // コントローラーやキーボードの十字キーでフォーカスされた時
    public void OnSelect(BaseEventData eventData)
    {
        PlaySound(SoundKeys.CursorMove);
    }

    // マウスカーソルが乗った時
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(SoundKeys.CursorMove);
    }

    // ==========================================
    // 決定音（クリック・エンターキー）
    // ==========================================

    // マウスでクリックされた時
    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(SoundKeys.ButtonPress);
    }

    // キーボードのEnterキーや、コントローラーの決定ボタンが押された時
    public void OnSubmit(BaseEventData eventData)
    {
        PlaySound(SoundKeys.ButtonPress);
    }

    // ==========================================
    // 共通の再生処理
    // ==========================================
    private void PlaySound(string soundKey)
    {
        // SoundPlayerが存在する場合のみ鳴らす（エラー防止）
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(soundKey);
        }
    }
}