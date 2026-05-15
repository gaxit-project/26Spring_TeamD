using UnityEngine;
using UnityEngine.EventSystems; // UIのイベントを扱うために必要

/// <summary>
/// ボタンにアタッチするだけで、フォーカスされた時やマウスが乗った時にカーソル移動音を鳴らすスクリプト
/// </summary>
public class UICursorMoveSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    // コントローラーやキーボードの十字キーでこのボタンが「選択（フォーカス）」されたときに呼ばれる
    public void OnSelect(BaseEventData eventData)
    {
        PlayCursorSound();
    }

    // マウスカーソルがこのボタンの上に「乗った」ときに呼ばれる
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayCursorSound();
    }

    private void PlayCursorSound()
    {
        // SoundPlayerが存在する場合のみ鳴らす（エラー防止）
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.CursorMove);
        }
    }
}