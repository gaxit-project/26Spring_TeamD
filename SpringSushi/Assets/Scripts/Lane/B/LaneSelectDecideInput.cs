using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// LaneSelectorUIの「決定」操作(Aボタン)専用の入力受け口。
/// </summary>
public class LaneSelectDecideInput : MonoBehaviour
{
    public static bool WasPressedThisFrame { get; private set; }

    // ★エラーになっている定義を追加
    public static bool InputDisabled { get; set; } = false;

    private static bool pressedThisFrame;

    private void Update()
    {
        WasPressedThisFrame = pressedThisFrame;
        pressedThisFrame = false;
    }

    public void OnLaneSelectDecide(InputValue value)
    {
        if (InputDisabled) return;
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused) return;

        if (value.isPressed)
        {
            pressedThisFrame = true;
        }
    }
}