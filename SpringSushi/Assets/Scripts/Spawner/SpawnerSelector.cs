using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LT/RTボタンで寿司選択・左スティック上下でSpawner選択を管理する。
/// GameManagerにアタッチする。
/// ※寿司生成は各SushiSpawnerが自動生成するため、RTでの手動生成は廃止。
/// </summary>
public class SpawnerSelector : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("スティックを倒し続けたときSpawner選択が切り替わる間隔（秒）")]
    [SerializeField] private float spawnerSwitchInterval = 0.4f;
    [Tooltip("スティックの入力を有効とみなすしきい値")]
    [SerializeField] private float stickDeadzone = 0.5f;

    private List<SushiSpawner> spawners = new();
    private int selectedSpawnerIndex = 0;
    private float spawnerSwitchTimer = 0f;

    // イベント（SpawnerHUDが購読）
    public event System.Action<int> OnSpawnerIndexChanged;   // 選択中Spawnerが変わった
    public event System.Action<SushiSpawner> OnSushiChanged; // 選択中寿司が変わった

    public SushiSpawner SelectedSpawner =>
        spawners.Count > 0 ? spawners[selectedSpawnerIndex] : null;

    private void OnEnable()
    {
        SpawnerInputManager.OnSushiShift += OnSushiShift;
    }

    private void OnDisable()
    {
        SpawnerInputManager.OnSushiShift -= OnSushiShift;
    }

    /// <summary>
    /// LT/RTボタンが押された瞬間に呼ばれる(SpawnerInputManager.OnSushiShiftLeft/Right経由)。
    /// dirは-1(LT)または1(RT)。
    /// </summary>
    private void OnSushiShift(int dir)
    {
        SelectedSpawner?.ShiftSelection(dir);
        if (SelectedSpawner != null)
            OnSushiChanged?.Invoke(SelectedSpawner);
    }

    private void Start()
    {
        spawners.AddRange(FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None));
        if (spawners.Count > 0)
            OnSpawnerIndexChanged?.Invoke(selectedSpawnerIndex);
    }

    private void Update()
    {
        // --- 左スティック上下：Spawner選択 ---
        float vertical = SpawnerInputManager.LeftStickValue.y;
        if (Mathf.Abs(vertical) > stickDeadzone)
        {
            spawnerSwitchTimer -= Time.deltaTime;
            if (spawnerSwitchTimer <= 0f)
            {
                int dir = vertical > 0 ? -1 : 1;
                selectedSpawnerIndex = (selectedSpawnerIndex + dir + spawners.Count) % spawners.Count;
                spawnerSwitchTimer = spawnerSwitchInterval;
                OnSpawnerIndexChanged?.Invoke(selectedSpawnerIndex);
            }
        }
        else
        {
            spawnerSwitchTimer = 0f;
        }
    }
}