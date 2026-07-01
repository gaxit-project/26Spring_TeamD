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
    [SerializeField] private float startX = -240f;
    [Tooltip("客のY座標")]
    [SerializeField] private float posY = -60f;

    [Header("入店設定")]
    [Tooltip("Wipe上のドアX座標（左側）。ここに到着したら実際に入店する")]
    [SerializeField] private float doorX = -480f;
    [Tooltip("Wipe上をドアまで歩く時間（秒）。Inspectorから変更可能")]
    [SerializeField] private float walkDuration = 1.5f;
    [Tooltip("★ 追加：入店アニメーションの終点X座標（ドアの向こう側・画面外）")]
    [SerializeField] private float exitX = -480f;
    [Tooltip("★ 追加：入店アニメーション（doorX → exitX）の時間（秒）")]
    [SerializeField] private float enterDuration = 0.3f;

    private readonly List<WipeCustomerEntry> queue = new();

    // ★ 追加：歩き中の客を管理（多重入店防止）
    private readonly HashSet<WipeCustomerEntry> walkingEntries = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 待機列に客を追加する。
    /// ★ 新規客はstartXに出現してからキュー位置へアニメーション移動する。
    /// </summary>
    public void EnqueueCustomer(WaitingCustomerData data)
    {
        var obj = Instantiate(wipeCustomerEntryPrefab, customerContainer);
        var entry = obj.GetComponent<WipeCustomerEntry>();
        entry.Initialize(data, defaultCustomerSprite);

        // ★ まずstartX（右端）に出現させる
        var rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(startX, posY);

        queue.Add(entry);

        // ★ キュー位置（doorX起点）へアニメーションで移動
        RefreshPositionsAnimated(entry);
    }

    /// <summary>
    /// 待機列の位置を整列し直す。
    /// ★ doorXを起点にする（ドア前から並ぶ）。
    /// </summary>
    private void RefreshPositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            var rect = queue[i].GetComponent<RectTransform>();
            // i=0（先頭）がdoorX、iが増えるほど右（startX方向）へずれる
            float x = doorX + (i * spacing);
            rect.anchoredPosition = new Vector2(x, posY);
        }
    }

    /// <summary>
    /// ★ 追加：新規客だけ現在位置からキュー位置へアニメーション移動させる。
    /// 他の客はRefreshPositionsで即座に整列し直す。
    /// </summary>
    private void RefreshPositionsAnimated(WipeCustomerEntry newEntry)
    {
        for (int i = 0; i < queue.Count; i++)
        {
            float targetX = doorX + (i * spacing);
            var entry = queue[i];

            if (entry == newEntry)
            {
                // 新規客だけアニメーションで移動（startX → キュー位置）
                entry.MoveToPosition(new Vector2(targetX, posY), walkDuration * 0.5f);
            }
            else
            {
                // 既存客は即座に整列（押し出し等が起きた場合も対応）
                var rect = entry.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(targetX, posY);
            }
        }
    }

    /// <summary>
    /// ★ 変更：先頭の客をドアまで歩かせ、到着後にonArrivedを呼ぶ。
    /// 「歩き中」の客は走行中フラグで管理し、二重呼び出しを防ぐ。
    /// </summary>
    public void BeginAdmit(System.Action<WaitingCustomerData> onArrived)
    {
        if (queue.Count == 0) return;

        var entry = queue[0];
        if (walkingEntries.Contains(entry)) return;

        queue.RemoveAt(0);
        walkingEntries.Add(entry);

        var data = entry.WaitingData;
        RefreshPositions();

        var rect = entry.GetComponent<RectTransform>();
        float currentX = rect.anchoredPosition.x;

        bool alreadyAtDoor = Mathf.Abs(currentX - doorX) < spacing * 0.5f;

        if (alreadyAtDoor)
        {
            // doorX → exitX（destroyOnComplete=true で最後にDestroy）
            entry.WalkToDoor(exitX, enterDuration, () =>
            {
                walkingEntries.Remove(entry);
                onArrived?.Invoke(data);
            }, destroyOnComplete: true);
        }
        else
        {
            // ★ 1段目：destroyOnComplete=false でオブジェクトを生かしたまま到着
            entry.WalkToDoor(doorX, walkDuration, () =>
            {
                // ★ 2段目：到着後にexitXへ移動してからDestroy
                entry.WalkToDoor(exitX, enterDuration, () =>
                {
                    walkingEntries.Remove(entry);
                    onArrived?.Invoke(data);
                }, destroyOnComplete: true);
            }, destroyOnComplete: false); // ← ここが修正の核心
        }
    }
    public int WaitingCount => queue.Count;
    public bool HasWaiting => queue.Count > 0;
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