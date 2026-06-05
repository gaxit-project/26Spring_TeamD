using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ワイプUIを管理する。
/// 待機列の表示・入店時のアニメーションを担当する。
/// </summary>
public class WipeCanvas : MonoBehaviour
{
    public static WipeCanvas Instance { get; private set; }

    [Header("UI参照")]
    [SerializeField] private RectTransform customerContainer; // 客を並べる親
    [SerializeField] private GameObject wipeCustomerEntryPrefab;

    [Header("客のデフォルトイラスト")]
    [SerializeField] private Sprite defaultCustomerSprite;

    [Header("並び設定")]
    [Tooltip("客同士の間隔（px）")]
    [SerializeField] private float spacing = 55f;
    [Tooltip("一番右の客のX座標（起点）")]
    [SerializeField] private float startX = 160f;
    [Tooltip("客のY座標")]
    [SerializeField] private float posY = -60f;

    private readonly List<WipeCustomerEntry> queue = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// CustomerManager から呼ばれる。待機列に客を追加する。
    /// </summary>
    public void EnqueueCustomer(WaitingCustomerData data)
    {
        var obj = Instantiate(wipeCustomerEntryPrefab, customerContainer);
        var entry = obj.GetComponent<WipeCustomerEntry>();
        entry.Initialize(data, defaultCustomerSprite);
        queue.Add(entry);
        RefreshPositions();
    }

    /// <summary>
    /// 先頭の客を入店させる。
    /// </summary>
    public WaitingCustomerData DequeueCustomer()
    {
        if (queue.Count == 0) return null;

        var entry = queue[0];
        queue.RemoveAt(0);

        var data = entry.WaitingData;

        // 入店アニメーション再生後にUI削除
        entry.PlayEnterAnimation(() => RefreshPositions());

        RefreshPositions();
        return data;
    }

    public int WaitingCount => queue.Count;

    public bool HasWaiting => queue.Count > 0;

    /// <summary>
    /// 待機列の位置を整列し直す。
    /// 右から並ぶ（ドアは左側）。
    /// </summary>
    private void RefreshPositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            var rect = queue[i].GetComponent<RectTransform>();

            // i=0（先頭）が startX になり、iが増える（後続）ほど spacing 分だけ右（+方向）にずれる
            float x = startX + (i * spacing);

            rect.anchoredPosition = new Vector2(x, posY);
        }
    }
}