using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SpawnerInputManager : MonoBehaviour
{
    public static Vector2 LeftStickValue { get; private set; }
    public static Vector2 RightStickValue { get; private set; }

    // ★ イベント名を入店用であることが分かりやすい名前に変更
    public static event Action OnAdmitCustomerPressed;
    public static event Action OnSpawnPressed;
    public static event Action<int> OnSushiShift;

    public void OnLeftStick(InputValue value)
    {
        LeftStickValue = value.Get<Vector2>();
    }

    public void OnRightStick(InputValue value)
    {
        RightStickValue = value.Get<Vector2>();
    }

    public void OnSpawn(InputValue value)
    {
        if (value.isPressed)
            OnSpawnPressed?.Invoke();
    }

    /// <summary>
    /// ★ 追加：LTボタン（Action名: AdmitCustomer）が押されたときに実行される
    /// </summary>
    public void OnAdmitCustomer(InputValue value)
    {
        if (value.isPressed)
            OnAdmitCustomerPressed?.Invoke();
    }

    public void OnSushiShiftLeft(InputValue value)
    {
        if (value.isPressed)
            OnSushiShift?.Invoke(-1);
    }

    public void OnSushiShiftRight(InputValue value)
    {
        if (value.isPressed)
            OnSushiShift?.Invoke(1);
    }
}