using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 右スティックでSpawner選択・寿司選択・RT生成を管理する。
/// GameManagerにアタッチする。
/// </summary>
public class SpawnerSelector : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("スティックを倒し続けたとき寿司選択が切り替わる間隔（秒）")]
    [SerializeField] private float sushiSwitchInterval = 0.3f;

    [Tooltip("スティックを倒し続けたときSpawner選択が切り替わる間隔（秒）")]
    [SerializeField] private float spawnerSwitchInterval = 0.4f;

    [Tooltip("スティックの入力を有効とみなすしきい値")]
    [SerializeField] private float stickDeadzone = 0.5f;

    private List<SushiSpawner> spawners = new();
    private int selectedSpawnerIndex = 0;

    private float sushiSwitchTimer = 0f;
    private float spawnerSwitchTimer = 0f;

    // イベント（SpawnerHUDが購読）
    public event System.Action<int> OnSpawnerIndexChanged;   // 選択中Spawnerが変わった
    public event System.Action<SushiSpawner> OnSushiChanged; // 選択中寿司が変わった

    public SushiSpawner SelectedSpawner =>
        spawners.Count > 0 ? spawners[selectedSpawnerIndex] : null;

    private void OnEnable()
    {
        SpawnerInputManager.OnSpawnPressed += OnSpawnPressed;
    }

    private void OnDisable()
    {
        SpawnerInputManager.OnSpawnPressed -= OnSpawnPressed;
    }

    private void Start()
    {
        // シーン内の全SushiSpawnerを収集
        spawners.AddRange(FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None));
        if (spawners.Count > 0)
            OnSpawnerIndexChanged?.Invoke(selectedSpawnerIndex);
    }

    private void Update()
    {
        // --- 左スティック左右：寿司選択 ---
        float horizontal = SpawnerInputManager.LeftStickValue.x; // ★ Left に変更
        if (Mathf.Abs(horizontal) > stickDeadzone)
        {
            sushiSwitchTimer -= Time.deltaTime;
            if (sushiSwitchTimer <= 0f)
            {
                int dir = horizontal > 0 ? 1 : -1;
                SelectedSpawner?.ShiftSelection(dir);
                sushiSwitchTimer = sushiSwitchInterval;
                if (SelectedSpawner != null)
                    OnSushiChanged?.Invoke(SelectedSpawner);
            }
        }
        else
        {
            sushiSwitchTimer = 0f;
        }

        // --- 左スティック上下：Spawner選択 ---
        float vertical = SpawnerInputManager.LeftStickValue.y; // ★ Left に変更
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

    private void OnSpawnPressed()
    {
        if (SelectedSpawner == null) return;
        bool spawned = SelectedSpawner.TryManualSpawn();
        if (spawned)
            Debug.Log($"[SpawnerSelector] 手動生成: {SelectedSpawner.SelectedSushi?.sushiName}");
    }
}