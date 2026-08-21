using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// LaneSelectorUIの「決定」操作(Aボタン)専用の入力受け口。
/// </summary>
public class LaneSelectDecideInput : MonoBehaviour
{
    // 外部から「このフレームで決定ボタンが押されたか？」を1回だけ取得できるようにする
    public static bool WasPressedThisFrame { get; private set; }

    //毎フレームの入力フラグをリセットするための一時変数
    private static bool pressedThisFrame;

    private void Update()
    {
        // 毎フレームの最初にフラグを同期し、消費する
        WasPressedThisFrame = pressedThisFrame;
        pressedThisFrame = false;
    }

    public void OnLaneSelectDecide(InputValue value)
    {
        // 押された瞬間（isPressedがtrueのタイミング）のみ反応する
        if (value.isPressed)
        {
            pressedThisFrame = true;
        }
    }
}