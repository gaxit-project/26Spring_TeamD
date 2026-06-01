using UnityEngine;

/// <summary>
/// コンボ数を管理するシングルトン。
/// </summary>
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    private int comboCount = 0;

    public int ComboCount => comboCount;

    // コンボ変化時に通知
    public event System.Action<int> OnComboChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 寿司の配達成功時に呼ぶ。
    /// </summary>
    public void IncrementCombo()
    {
        comboCount++;
        OnComboChanged?.Invoke(comboCount);
        Debug.Log($"<color=orange>[Combo]</color> {comboCount} コンボ");
    }

    /// <summary>
    /// 皿衝突 or Angry退場時に呼ぶ。
    /// </summary>
    public void ResetCombo()
    {
        if (comboCount == 0) return;
        comboCount = 0;
        OnComboChanged?.Invoke(comboCount);
        Debug.Log("<color=red>[Combo]</color> コンボリセット");
    }
}