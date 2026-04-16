using UnityEngine;
using UnityEngine.InputSystem;
using System; // Actionを使うために追加

public class LaneInputManager : MonoBehaviour
{
    // 「ボタンが押されたよ！」という通知を送るための窓口
    // 他のクラス（LaneNetworkなど）がこれを受け取る
    public static event Action<LaneColor> OnLaneButtonPressed;

    public void OnLaneBlue(InputValue value) { if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Blue); }
    public void OnLaneYellow(InputValue value) { if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Yellow); }
    public void OnLaneGreen(InputValue value) { if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Green); }
    public void OnLaneRed(InputValue value) { if (value.isPressed) OnLaneButtonPressed?.Invoke(LaneColor.Red); }
}