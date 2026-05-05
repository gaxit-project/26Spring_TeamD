using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD_Canvas上のCustomer UI全体を管理する。
/// CustomerAIが生成されるたびにCustomerHUDEntryを生成して追跡する。
/// </summary>
public class CustomerHUD : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject hudEntryPrefab; // CustomerHUDEntryがついたPrefab

    private readonly List<CustomerHUDEntry> entries = new();

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// CustomerManagerから呼ばれる。新しいCustomerAIのHUDEntryを生成する。
    /// </summary>
    public void RegisterCustomer(CustomerAI customer)
    {
        if (hudEntryPrefab == null || hudCanvas == null) return;

        var obj = Instantiate(hudEntryPrefab, hudCanvas.transform);
        var entry = obj.GetComponent<CustomerHUDEntry>();
        if (entry == null) return;

        entry.Initialize(customer, hudCanvas, mainCamera);
        entries.Add(entry);

        // 削除時にリストからも除く
        customer.OnStateChanged += ai =>
        {
            if (ai.State == CustomerAI.CustomerState.Leaving)
                entries.Remove(entry);
        };
    }
}