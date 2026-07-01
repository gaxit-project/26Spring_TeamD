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
    [SerializeField] private RectTransform customerContainer;
    [SerializeField] private GameObject wipeCustomerEntryPrefab;

    [Header("客のデフォルトイラスト")]
    [SerializeField] private Sprite defaultCustomerSprite;

    [Header("並び設定")]
    [Tooltip("客同士の間隔（px）")]
    [SerializeField] private float spacing = 55f;
    [Tooltip("一番右の客の生成X座標（起点）")]
    [SerializeField] private float startX = 160f;
    [Tooltip("客のY座標")]
    [SerializeField] private float posY = -60f;

    [Header("入店設定")]
    [Tooltip("Wipe上のドアX座標（左側）。ここに到着したら実際に入店する")]
    [SerializeField] private float doorX = -160f;
    [Tooltip("Wipe上をドアまで歩く時間（秒）。Inspectorから変更可能")]
    [SerializeField] private float walkDuration = 1.5f;

    private readonly List<WipeCustomerEntry> queue = new();

    // ★ 追加：歩き中の客を管理（多重入店防止）
    private readonly HashSet<WipeCustomerEntry> walkingEntries = new();

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
    /// ★ 変更：先頭の客をドアまで歩かせ、到着後にonArrivedを呼ぶ。
    /// 「歩き中」の客は走行中フラグで管理し、二重呼び出しを防ぐ。
    /// </summary>
    public void BeginAdmit(System.Action<WaitingCustomerData> onArrived)
    {
        if (queue.Count == 0) return;

        var entry = queue[0];

        // ★ すでに歩き中の客は再度歩かせない
        if (walkingEntries.Contains(entry)) return;

        queue.RemoveAt(0);
        walkingEntries.Add(entry);

        var data = entry.WaitingData;

        RefreshPositions();

        // ★ ドアまで歩く → 到着してからゲーム内スポーン
        entry.WalkToDoor(doorX, walkDuration, () =>
        {
            walkingEntries.Remove(entry);
            onArrived?.Invoke(data);
        });
    }

    public int WaitingCount => queue.Count;
    public bool HasWaiting => queue.Count > 0;

    private void RefreshPositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            var rect = queue[i].GetComponent<RectTransform>();
            float x = startX + (i * spacing);
            rect.anchoredPosition = new Vector2(x, posY);
        }
    }

    // ───────────────────────────────────────────────
    // ★ 旧・即時デキュー。BeginAdmitに置き換えたためコメントアウト
    // ───────────────────────────────────────────────
    /*
    public WaitingCustomerData DequeueCustomer()
    {
        if (queue.Count == 0) return null;
        var entry = queue[0];
        queue.RemoveAt(0);
        var data = entry.WaitingData;
        entry.PlayEnterAnimation(() => RefreshPositions());
        RefreshPositions();
        return data;
    }
    */
}