using System.Collections;
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
    [Tooltip("客同士の間隔（px）。待機列の並び・グループ移動時の間隔の両方に使う")]
    [SerializeField] private float spacing = 50f;
    [Tooltip("一番右の客の生成X座標（起点）")]
    [SerializeField] private float startX = -240f;
    [Tooltip("客のY座標")]
    [SerializeField] private float posY = -60f;
    [Tooltip("待機列の再整列(詰め直し)にかかる時間（秒）")]
    [SerializeField] private float realignDuration = 0.3f;

    [Header("入店設定（単独入店用）")]
    [Tooltip("Wipe上のドアX座標（左側）。ここに到着したら実際に入店する")]
    [SerializeField] private float doorX = -480f;
    [Tooltip("Wipe上をドアまで歩く時間（秒）。Inspectorから変更可能")]
    [SerializeField] private float walkDuration = 1.5f;
    [Tooltip("入店アニメーションの終点X座標（ドアの向こう側・画面外）")]
    [SerializeField] private float exitX = -480f;
    [Tooltip("単独入店時、ドア直前から画面外まで抜ける短い演出時間（秒）")]
    [SerializeField] private float enterDuration = 0.3f;

    [Header("グループ入店設定")]
    [Tooltip("グループ入店時、ドア通過(退場)を1人ずつずらす間隔（秒）。例：1にすると1番目退場の1秒後に2番目が退場する")]
    [SerializeField] private float groupExitStagger = 1f;
    [Tooltip("グループ入店時、ドア前からexitXまで実際に歩くのにかかる時間（秒）。enterDurationとは別に管理し、必ず視認できる速度で歩かせる")]
    [SerializeField] private float groupExitWalkDuration = 0.8f;

    private readonly List<WipeCustomerEntry> queue = new();

    // ★ 追加：歩き中の客を管理（多重入店防止）
    private readonly HashSet<WipeCustomerEntry> walkingEntries = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 待機列に客を1人追加する。
    /// </summary>
    public void EnqueueCustomer(WaitingCustomerData data)
    {
        EnqueueCustomerBatch(new List<WaitingCustomerData> { data });
    }

    /// <summary>
    /// 待機列に客をまとめて追加する。
    /// 同じ波(wave)の客は、生成した瞬間からspacing間隔で離れた位置に配置することで、
    /// 同一フレームで複数生成しても出発点で重なって見えないようにする。
    /// 全員追加し終えた後、1回だけキュー位置への整列アニメーションをかける。
    /// </summary>
    public void EnqueueCustomerBatch(List<WaitingCustomerData> dataList)
    {
        if (dataList == null || dataList.Count == 0) return;

        for (int i = 0; i < dataList.Count; i++)
        {
            var obj = Instantiate(wipeCustomerEntryPrefab, customerContainer);
            var entry = obj.GetComponent<WipeCustomerEntry>();
            entry.Initialize(dataList[i], defaultCustomerSprite);

            var rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(startX + i * spacing, posY);

            queue.Add(entry);
        }

        RefreshPositions();
    }

    /// <summary>
    /// 待機列の位置を整列し直す。doorXを起点にする（ドア前から並ぶ）。
    /// 既存客もアニメーションで動かすことで、テレポートによる重なりを防ぐ。
    /// </summary>
    private void RefreshPositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            float targetX = doorX + (i * spacing);
            var entry = queue[i];
            entry.MoveToPosition(new Vector2(targetX, posY), realignDuration);
        }
    }

    /// <summary>
    /// 先頭の客をドアまで歩かせ、到着後にonArrivedを呼ぶ。
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
            entry.WalkToDoor(exitX, enterDuration, () =>
            {
                walkingEntries.Remove(entry);
                onArrived?.Invoke(data);
            }, destroyOnComplete: true);
        }
        else
        {
            entry.WalkToDoor(doorX, walkDuration, () =>
            {
                entry.WalkToDoor(exitX, enterDuration, () =>
                {
                    walkingEntries.Remove(entry);
                    onArrived?.Invoke(data);
                }, destroyOnComplete: true);
            }, destroyOnComplete: false);
        }
    }

    /// <summary>
    /// 先頭からcount人を、隊列(spacing間隔)を保ったまま同時にドア前まで歩かせ、
    /// その後1人ずつgroupExitStaggerだけ間を空けてドアを通過・退場させる。
    /// 退場時の移動はgroupExitWalkDuration(必ず視認できる速さ)を使い、
    /// enterDuration(単独入店用の一瞬の演出値)には依存しない。
    /// onArrivedCallbacksは待機列の先頭側から順に対応する(callbacks[0]が一番手前の客)。
    /// </summary>
    public void BeginAdmitBurst(int count, List<System.Action<WaitingCustomerData>> onArrivedCallbacks)
    {
        int actualCount = Mathf.Min(count, queue.Count, onArrivedCallbacks.Count);
        if (actualCount <= 0) return;

        var entries = new List<WipeCustomerEntry>();
        for (int i = 0; i < actualCount; i++)
            entries.Add(queue[i]);

        foreach (var entry in entries)
        {
            queue.Remove(entry);
            walkingEntries.Add(entry);
        }

        RefreshPositions();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var data = entry.WaitingData;
            var callback = onArrivedCallbacks[i];

            float lineupX = doorX + (i * spacing);
            float exitTargetX = exitX + (i * spacing);
            float exitDelay = i * groupExitStagger;

            var rect = entry.GetComponent<RectTransform>();
            bool alreadyAtLineup = Mathf.Abs(rect.anchoredPosition.x - lineupX) < spacing * 0.5f;

            if (alreadyAtLineup)
            {
                StartCoroutine(ExitAfterDelay(entry, data, callback, exitTargetX, exitDelay));
            }
            else
            {
                entry.WalkToDoor(lineupX, walkDuration, () =>
                {
                    StartCoroutine(ExitAfterDelay(entry, data, callback, exitTargetX, exitDelay));
                }, destroyOnComplete: false);
            }
        }
    }

    /// <summary>
    /// delay秒待ってから、groupExitWalkDurationをかけてドアを通過・退場する(フェーズ2)。
    /// targetXはこの客専用の目的地(隊列オフセットを保った位置)。
    /// </summary>
    private IEnumerator ExitAfterDelay(WipeCustomerEntry entry, WaitingCustomerData data,
                                        System.Action<WaitingCustomerData> callback, float targetX, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        entry.WalkToDoor(targetX, groupExitWalkDuration, () =>
        {
            walkingEntries.Remove(entry);
            callback?.Invoke(data);
        }, destroyOnComplete: true);
    }

    public int WaitingCount => queue.Count;
    public bool HasWaiting => queue.Count > 0;
}