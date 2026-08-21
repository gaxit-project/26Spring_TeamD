using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class LaneInputManager : MonoBehaviour
{
    public static event Action<LaneColor> OnLaneButtonPressed;

    /// <summary>
    /// trueの間、ダイレクト方式(X/Y/A/B)のボタン入力を無視する。
    /// 選択方式(LaneSelectorUI)が有効なモードのときに、外部からtrueに設定する。
    /// </summary>
    public static bool DirectInputDisabled { get; set; } = false;

    // 共通の入力受付判定（プレイ中かつダイレクト入力が無効化されていない場合のみtrue）
    private bool CanAcceptInput()
    {
        if (DirectInputDisabled) return false;

        // ★ GameStateManagerが存在し、かつ「Playing（プレイ中）」でなければ入力を受け付けない
        // （Title, Ready, Paused, Result などではすべて弾かれます）
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying)
        {
            return false;
        }

        return true;
    }

    public void OnLaneBlue(InputValue value)
    {
        if (!CanAcceptInput()) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Blue);
    }

    public void OnLaneYellow(InputValue value)
    {
        if (!CanAcceptInput()) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Yellow);
    }

    public void OnLaneGreen(InputValue value)
    {
        if (!CanAcceptInput()) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Green);
    }

    public void OnLaneRed(InputValue value)
    {
        if (!CanAcceptInput()) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Red);
    }
}