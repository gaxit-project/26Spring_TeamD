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

    public void OnLaneBlue(InputValue value)
    {
        if (DirectInputDisabled) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Blue);
    }

    public void OnLaneYellow(InputValue value)
    {
        if (DirectInputDisabled) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Yellow);
    }

    public void OnLaneGreen(InputValue value)
    {
        if (DirectInputDisabled) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Green);
    }

    public void OnLaneRed(InputValue value)
    {
        if (DirectInputDisabled) return;
        if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Red);
    }
}