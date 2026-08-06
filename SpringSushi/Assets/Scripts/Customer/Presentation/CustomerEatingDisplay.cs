using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 机の上に食べている寿司の3Dモデルを表示する。
/// 「どこに置くか」は座席側のSeatPlateAnchor.plateSlotsを参照し、
/// 配達された順に1つずつスロットへ追加していく。
/// 新しいバッチが始まったタイミング(OnBatchStarted)で机の上をリセットし、
/// 退店・満足・怒りのタイミングでも片付ける。
/// 同じバッチ内で次の寿司を待っている間(Eating→Ordering)は片付けない。
/// </summary>
public class CustomerEatingDisplay : MonoBehaviour
{
    private readonly List<GameObject> activeModels = new();
    private List<Transform> plateSlots = new();
    private Transform fallbackAnchor;

    private void Awake()
    {
        fallbackAnchor = transform;
    }

    public void Bind(CustomerStateMachine stateMachine)
    {
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    /// <summary>
    /// 寿司を置く場所のリストを外部(座席側)から設定する。
    /// CustomerAI.Initialize()で、着席する座席のSeatPlateAnchorから渡される。
    /// </summary>
    public void SetPlateSlots(List<Transform> slots)
    {
        plateSlots = (slots != null && slots.Count > 0) ? slots : new List<Transform>();
    }

    /// <summary>
    /// 新しいバッチが始まったタイミングで机の上をリセットする。
    /// CustomerOrderFlowService.OnBatchStartedから呼ばれる。
    /// </summary>
    public void ClearForNewBatch() => ClearAll();

    /// <summary>
    /// 1皿分の寿司モデルを、次に空いているスロットへ配置する。
    /// CustomerOrderFlowService.OnItemServedから呼ばれる。
    /// </summary>
    public void AddServedItem(CustomerOrder order)
    {
        if (order?.sushiData == null || order.sushiData.sushiModel == null) return;

        Transform parent;
        if (plateSlots.Count > 0)
        {
            parent = plateSlots[activeModels.Count % plateSlots.Count]; // スロット数を超えたら先頭から使い回す
        }
        else
        {
            Debug.LogWarning($"[CustomerEatingDisplay] plateSlotsが未設定のため、{fallbackAnchor.name}の位置に表示します。");
            parent = fallbackAnchor;
        }

        var model = Instantiate(order.sushiData.sushiModel, parent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        activeModels.Add(model);
    }

    private void HandleStateChanged(CustomerAI.CustomerState newState)
    {
        // 退店・満足・怒りのタイミングでのみ片付ける。
        // Eating→Orderingへの遷移(同じバッチ内で次の寿司を待つだけ)では片付けない。
        if (newState == CustomerAI.CustomerState.Leaving ||
            newState == CustomerAI.CustomerState.Satisfied ||
            newState == CustomerAI.CustomerState.Angry)
        {
            ClearAll();
        }
    }

    private void ClearAll()
    {
        foreach (var model in activeModels)
            if (model != null) Destroy(model);

        activeModels.Clear();
    }

    private void OnDestroy() => ClearAll();
}