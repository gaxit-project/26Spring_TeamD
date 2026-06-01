using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    public RectTransform root;
    public HorizontalLayoutGroup orderImageContainer;
    public GameObject orderImagePrefab;
    public PatienceGaugeBubble patienceGauge; // ← 新コンポーネントに変更
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
        patienceGauge.SetValue(1f); // ← 初期値

        root.gameObject.SetActive(false);
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
        if (customer == null) { Destroy(gameObject); return; }
        FollowCustomer();
    }

    private void FollowCustomer()
    {
        if (customer.Seat == null) return;

        Vector3 worldPos = customer.Seat.position + Vector3.up * 2f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        bool behindCamera = screenPos.z < 0f;
        root.gameObject.SetActive(!behindCamera);
        if (behindCamera) return;

        if (hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            root.position = screenPos;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hudCanvas.transform as RectTransform,
                screenPos, hudCanvas.worldCamera,
                out Vector2 localPoint);
            root.localPosition = localPoint;
        }
    }

    private void OnStateChanged(CustomerAI ai)
    {
        switch (ai.State)
        {
            case CustomerAI.CustomerState.Ordering:
            case CustomerAI.CustomerState.Eating:
                root.gameObject.SetActive(true);
                break;
            case CustomerAI.CustomerState.Satisfied:
                root.gameObject.SetActive(true);
                Invoke(nameof(DestroySelf), 1.5f);
                break;
            case CustomerAI.CustomerState.Leaving:
                root.gameObject.SetActive(false);
                Invoke(nameof(DestroySelf), 0.1f);
                break;
            default:
                root.gameObject.SetActive(false);
                break;
        }
    }

    private void OnOrderUpdated(CustomerAI ai)
    {
        UpdateOrderImages();
        satisfiedSlider.value = ai.SatisfiedRate;
    }

    private void OnPatienceChanged(CustomerAI ai)
    {
        patienceGauge.SetValue(ai.PatienceRate); // ← 変更
    }

    private void UpdateOrderImages()
    {
        if (customer == null) return;
        var batch = customer.OrderQueue.CurrentBatch;

        foreach (var img in orderImages) Destroy(img.gameObject);
        orderImages.Clear();

        foreach (var order in batch)
        {
            var obj = Instantiate(orderImagePrefab, orderImageContainer.transform);
            var img = obj.GetComponent<Image>();
            if (img != null && order.sushiData != null)
            {
                img.sprite = order.sushiData.sushiIcon;
                img.color = order.isDelivered ? Color.gray : Color.white;
            }
            orderImages.Add(img);
        }
    }

    private void DestroySelf() => Destroy(gameObject);
}