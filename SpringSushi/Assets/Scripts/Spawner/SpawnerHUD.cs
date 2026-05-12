using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全SushiSpawnerのHUDEntryを管理する。
/// HUD_CanvasにアタッチしてSpawnerSelectorのイベントを購読する。
/// </summary>
public class SpawnerHUD : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject entryPrefab; // SpawnerHUDEntryがついたPrefab
    [SerializeField] private SpawnerSelector selector;

    private readonly List<SpawnerHUDEntry> entries = new();

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // シーン内の全SushiSpawnerに対してEntryを生成
        var spawners = FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            var obj = Instantiate(entryPrefab, hudCanvas.transform);
            var entry = obj.GetComponent<SpawnerHUDEntry>();
            entry.Initialize(spawner, hudCanvas, mainCamera);
            entries.Add(entry);
        }

        // 初期選択を反映
        if (entries.Count > 0)
            entries[0].SetSelected(true);

        // Selectorのイベントを購読
        if (selector != null)
        {
            selector.OnSpawnerIndexChanged += OnSpawnerIndexChanged;
            selector.OnSushiChanged += OnSushiChanged;
        }
    }

    private void OnDestroy()
    {
        if (selector != null)
        {
            selector.OnSpawnerIndexChanged -= OnSpawnerIndexChanged;
            selector.OnSushiChanged -= OnSushiChanged;
        }
    }

    private void OnSpawnerIndexChanged(int index)
    {
        for (int i = 0; i < entries.Count; i++)
            entries[i].SetSelected(i == index);
    }

    private void OnSushiChanged(SushiSpawner spawner)
    {
        // 該当するEntryのアイコンを更新
        var spawners = FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None);
        for (int i = 0; i < spawners.Length && i < entries.Count; i++)
        {
            if (spawners[i] == spawner)
            {
                entries[i].UpdateIcon();
                break;
            }
        }
    }
}