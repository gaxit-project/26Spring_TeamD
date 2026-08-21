using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SpawnerInputManager : MonoBehaviour
{
    public static Vector2 LeftStickValue { get; private set; }
    public static Vector2 RightStickValue { get; private set; }

    public static event Action OnAdmitCustomerPressed;
    public static event Action OnSpawnPressed;
    public static event Action<int> OnSushiShift;

    public void OnLeftStick(InputValue value)
    {
        // ★ ポーズ中ならスティック入力を無視する
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused)
        {
            LeftStickValue = Vector2.zero;
            return;
        }
        LeftStickValue = value.Get<Vector2>();
    }

    public void OnRightStick(InputValue value)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused)
        {
            RightStickValue = Vector2.zero;
            return;
        }
        RightStickValue = value.Get<Vector2>();
    }

    public void OnSpawn(InputValue value)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused) return;
        if (value.isPressed)
            OnSpawnPressed?.Invoke();
    }

    public void OnAdmitCustomer(InputValue value)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused) return;
        if (value.isPressed)
            OnAdmitCustomerPressed?.Invoke();
    }

    public void OnSushiShiftLeft(InputValue value)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused) return;
        if (value.isPressed)
            OnSushiShift?.Invoke(-1);
    }

    public void OnSushiShiftRight(InputValue value)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused) return;
        if (value.isPressed)
            OnSushiShift?.Invoke(1);
    }
}