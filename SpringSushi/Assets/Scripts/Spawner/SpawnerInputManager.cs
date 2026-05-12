using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// InputSystemからSpawner操作に必要な入力を受け取りイベントで通知する。
/// GameManagerなどにアタッチしてInputActionAssetと接続する。
/// </summary>
public class SpawnerInputManager : MonoBehaviour
{
    // 左スティックの軸値（Spawner選択用）
    public static Vector2 LeftStickValue { get; private set; }

    // 右スティックの軸値（寿司選択用）
    public static Vector2 RightStickValue { get; private set; }

    // RTボタン押下イベント
    public static event Action OnSpawnPressed;

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
}