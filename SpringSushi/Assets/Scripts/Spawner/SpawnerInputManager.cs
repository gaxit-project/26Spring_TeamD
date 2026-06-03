using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SpawnerInputManager : MonoBehaviour
{
    public static Vector2 LeftStickValue { get; private set; }
    public static Vector2 RightStickValue { get; private set; }

    public static event Action OnSpawnPressed;
    public static event Action<int> OnSushiShift; // ★ 追加：左右矢印キー用

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

    // ★ 追加：InputActionAssetのAction名は "SushiShiftLeft" / "SushiShiftRight" に対応
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