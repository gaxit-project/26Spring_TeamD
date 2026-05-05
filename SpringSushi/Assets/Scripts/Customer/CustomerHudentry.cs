using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD_Canvas上の1客分のUI。
/// CustomerAIのイベントを購読してUIを更新する。
/// </summary>
public class CustomerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    public RectTransform root;
    public HorizontalLayoutGroup orderImageContainer;
    public GameObject orderImagePrefab;  // SushiIconを表示するImage Prefab
    public Slider patienceSlider;
    public Slider satisfiedSlider;

    private CustomerAI customer;
    private Camera mainCamera;
    private Canvas hudCanvas;
    private readonly List<Image> orderImages = new();

    public void Initialize(CustomerAI ai, Canvas canvas, Camera cam)
    {
        customer = ai;
        hudCanvas = canvas;
        mainCamera = cam;

        customer.OnStateChanged += OnStateChanged;
        customer.OnOrderUpdated += OnOrderUpdated;
        customer.OnPatienceChanged += OnPatienceChanged;

        satisfiedSlider.value = 0f;
        patienceSlider.value = 1f;

        UpdateOrderImages();
    }

    private void OnDestroy()
    {
        if (customer == null) return;
        customer.OnStateChanged -= OnStateChanged;
        customer.OnOrderUpdated -= OnOrderUpdated;
        customer.OnPatienceChanged -= OnPatienceChanged;
    }

    private void LateUpdate()
    {
        if (customer == null)
        {
            Destroy(gameObject);
            return;
        }
        FollowCustomer();
    }

    /// <summary>
    /// お客さんの頭上にUIを追従させる。
    /// </summary>
    private void FollowCustomer()
    {
        if (customer.Seat == null) return;

        // SeatのWorld座標をScreen座標に変換
        Vector3 worldPos = customer.Seat.position + Vector3.up * 2f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // カメラの後ろにいる場合は非表示
        bool behindCamera = screenPos.z < 0f;
        root.gameObject.SetActive(!behindCamera);
        if (behindCamera) return;

        // CanvasのRenderMode対応
        if (hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            root.position = screenPos;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hudCanvas.transform as RectTransform,
                screenPos,
                hudCanvas.worldCamera,
                out Vector2 localPoint
            );
            root.localPosition = localPoint;
        }
    }

    private void OnStateChanged(CustomerAI ai)
    {
        bool show = ai.State == CustomerAI.CustomerState.Ordering ||
                    ai.State == CustomerAI.CustomerState.Satisfied;
        root.gameObject.SetActive(show);

        if (ai.State == CustomerAI.CustomerState.Leaving ||
            ai.State == CustomerAI.CustomerState.Satisfied)
        {
            Invoke(nameof(DestroySelf), 1.5f);
        }
    }

    private void OnOrderUpdated(CustomerAI ai)
    {
        UpdateOrderImages();
        satisfiedSlider.value = ai.SatisfiedRate;
    }

    private void OnPatienceChanged(CustomerAI ai)
    {
        patienceSlider.value = ai.PatienceRate;
    }

    private void UpdateOrderImages()
    {
        if (customer == null) return;

        var batch = customer.OrderQueue.CurrentBatch;

        // 既存ImageをClear
        foreach (var img in orderImages)
            Destroy(img.gameObject);
        orderImages.Clear();

        // バッチ分のImageを生成
        foreach (var order in batch)
        {
            var obj = Instantiate(orderImagePrefab, orderImageContainer.transform);
            var img = obj.GetComponent<Image>();
            if (img != null && order.sushiData != null)
            {
                img.sprite = order.sushiData.sushiIcon;
                // 届いた注文はグレーアウト
                img.color = order.isDelivered ? Color.gray : Color.white;
            }
            orderImages.Add(img);
        }
    }

    private void DestroySelf() => Destroy(gameObject);
}