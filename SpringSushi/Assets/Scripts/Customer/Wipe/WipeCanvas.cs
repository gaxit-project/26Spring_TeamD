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
    [Tooltip("客同士の間隔（px）")]
    [SerializeField] private float spacing = 50f;
    [Tooltip("一番右の客の生成X座標（起点）")]
    [SerializeField] private float startX = -240f;
    [Tooltip("客のY座標")]
    [SerializeField] private float posY = -60f;

    [Header("移動速度設定（共通）")]
    [Tooltip("客が歩く速度（px/秒）。待機列の整列・単独入店・グループ入店、" +
             "すべてこの1つの値で速度を統一する")]
    [SerializeField] private float customerWalkSpeed = 200f;

    [Header("入店設定")]
    [Tooltip("Wipe上のドアX座標（左側）。ここに到着したら実際に入店する")]
    [SerializeField] private float doorX = -480f;
    [Tooltip("入店アニメーションの終点X座標（ドアの向こう側・画面外）")]
    [SerializeField] private float exitX = -480f;
    [Tooltip("ドア直前から画面外まで抜ける短い演出時間（秒）。これは距離に関係ない一瞬のポップ演出")]
    [SerializeField] private float enterDuration = 1.0f;

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
    /// customerWalkSpeedを使い、距離÷速度で移動時間を計算する。
    /// </summary>
    private void RefreshPositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            float targetX = doorX + (i * spacing);
            var entry = queue[i];
            var rect = entry.GetComponent<RectTransform>();
            float distance = Mathf.Abs(rect.anchoredPosition.x - targetX);
            float duration = customerWalkSpeed > 0f ? distance / customerWalkSpeed : 0f;
            entry.MoveToPosition(new Vector2(targetX, posY), duration);
        }
    }

    /// <summary>
    /// 先頭の客をドアまで歩かせ、到着後にonArrivedを呼ぶ。
    /// customerWalkSpeedを使い、距離÷速度で移動時間を計算するため、
    /// 待機列のどの位置にいてもグループ入店時と同じ速度で歩く。
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
        float distance = Mathf.Abs(rect.anchoredPosition.x - doorX);
        float duration = customerWalkSpeed > 0f ? distance / customerWalkSpeed : 0f;

        entry.WalkToDoor(doorX, duration, () =>
        {
            entry.WalkToDoor(exitX, enterDuration, () =>
            {
                walkingEntries.Remove(entry);
                onArrived?.Invoke(data);
            }, destroyOnComplete: true);
        }, destroyOnComplete: false);
    }

    /// <summary>
    /// 先頭からcount人を、隊列(spacing間隔)を保ったまま毎フレーム同じ歩幅で
    /// ドアへ向かって進める。Lerp補間ではなく「全員が同じ速度で同時に歩く」
    /// 方式にすることで、数学的に間隔が常に一定に保たれ、重なりが起こらない。
    /// ドア(doorX)に到達した客から、その場でexitXへ抜けて退場する。
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

        // 内部コルーチン(MoveToPosition等)を止めてから手動移動(GroupWalkRoutine)に切り替える。
        // 位置の強制スナップは行わない(生成直後のstartX→doorXへの入場演出を壊さないため)。
        foreach (var entry in entries)
            entry.StopManagedMove();

        StartCoroutine(GroupWalkRoutine(entries, onArrivedCallbacks));
    }
    /// <summary>
    /// グループ全員を毎フレーム同じ距離だけ進める(定速直線運動)。
    /// ドア(doorX)を越えた客から順に取り除き、exitXへの短いポップ演出を経て退場させる。
    /// 全員が全く同じ歩幅で動くため、間隔(spacing)は開始から終了まで常に一定であり、
    /// 重なりが原理的に発生しない。
    /// </summary>
    private IEnumerator GroupWalkRoutine(List<WipeCustomerEntry> entries,
                                      List<System.Action<WaitingCustomerData>> callbacks)
    {
        var pending = new List<(WipeCustomerEntry entry, WaitingCustomerData data, System.Action<WaitingCustomerData> callback)>();
        for (int i = 0; i < entries.Count; i++)
        {
            pending.Add((entries[i], entries[i].WaitingData, callbacks[i]));
        }

        while (pending.Count > 0 && customerWalkSpeed > 0f)
        {
            float step = customerWalkSpeed * Time.deltaTime;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (entry, data, callback) = pending[i];
                if (entry == null) { pending.RemoveAt(i); continue; }

                var rect = entry.GetComponent<RectTransform>();
                float newX = rect.anchoredPosition.x - step;
                rect.anchoredPosition = new Vector2(newX, rect.anchoredPosition.y);

                if (newX <= doorX)
                {
                    pending.RemoveAt(i);
                    entry.WalkToDoor(exitX, enterDuration, () =>
                    {
                        walkingEntries.Remove(entry);
                        callback?.Invoke(data);
                    }, destroyOnComplete: true);
                }
            }

            yield return null;
        }
    }

    public int WaitingCount => queue.Count;
    public bool HasWaiting => queue.Count > 0;
}