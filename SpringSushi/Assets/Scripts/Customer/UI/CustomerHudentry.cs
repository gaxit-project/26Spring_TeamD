using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    public RectTransform root;
    public GridLayoutGroup orderImageContainer; // ★ VerticalLayoutGroup → GridLayoutGroup に変更
    public GameObject orderImagePrefab;
    public PatienceGaugeBubble patienceGauge;
    public Slider satisfiedSlider;

    [Header("注文アイコン設定")]
    public float baseIconSize = 48f;
    public float minIconSize = 20f;
    public float iconSpacing = 4f;
    public int maxColumns = 2;
    public Vector2 maxContainerSize = new Vector2(100f, 100f);

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
        customer.OnOrderPhaseChanged += OnOrderPhaseChanged;

        satisfiedSlider.value = 0f;
        patienceGauge.SetValue(1f);
        root.gameObject.SetActive(false);
        UpdateOrderImages();
    }

    private void OnDestroy()
    {
        if (customer == null) return;
        customer.OnStateChanged -= OnStateChanged;
        customer.OnOrderUpdated -= OnOrderUpdated;
        customer.OnPatienceChanged -= OnPatienceChanged;
        customer.OnOrderPhaseChanged -= OnOrderPhaseChanged;
    }

    private void OnOrderPhaseChanged(CustomerAI ai)
    {
        patienceGauge.SetOrderPhase(ai.Phase);
        if (ai.Phase == CustomerAI.OrderPhase.Waiting)
            patienceGauge.ResetToFull();
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
            root.position = screenPos;
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
                root.gameObject.SetActive(true);
                ShowOrderImages(true);
                patienceGauge.ResetToFull();
                patienceGauge.SetOrderPhase(CustomerAI.OrderPhase.Waiting);
                break;

            case CustomerAI.CustomerState.Eating:
                root.gameObject.SetActive(true);
                ShowOrderImages(false);
                patienceGauge.SetOrderPhase(ai.Phase);
                break;

            case CustomerAI.CustomerState.Angry:
                root.gameObject.SetActive(false);
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

    private void ShowOrderImages(bool visible)
    {
        foreach (var img in orderImages)
            if (img != null) img.gameObject.SetActive(visible);
    }

    private void OnOrderUpdated(CustomerAI ai)
    {
        UpdateOrderImages();
        satisfiedSlider.value = ai.SatisfiedRate;
    }

    private void OnPatienceChanged(CustomerAI ai)
    {
        patienceGauge.SetValue(ai.PatienceRate);
    }

    private void UpdateOrderImages()
    {
        if (customer == null) return;
        if (orderImageContainer == null) return;

        var batch = customer.OrderQueue?.CurrentBatch;

        foreach (var img in orderImages)
            if (img != null) Destroy(img.gameObject);
        orderImages.Clear();

        var grid = orderImageContainer;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.spacing = new Vector2(iconSpacing, iconSpacing);

        var pendingOrders = batch?.Where(o => !o.isDelivered).ToList() ?? new();
        int count = pendingOrders.Count;

        // ★ ここに追加
        Debug.Log($"[HUD] pendingOrders={count}, batch null? {batch == null}");

        int columns = count <= 1 ? 1 : Mathf.Min(maxColumns, count);
        int rows = count > 0 ? Mathf.CeilToInt((float)count / columns) : 1;

        float sizeByWidth = (maxContainerSize.x - (columns - 1) * iconSpacing) / columns;
        float sizeByHeight = (maxContainerSize.y - (rows - 1) * iconSpacing) / rows;
        float iconSize = Mathf.Clamp(Mathf.Min(baseIconSize, sizeByWidth, sizeByHeight), minIconSize, baseIconSize);

        // ★ こちらも追加
        Debug.Log($"[HUD] columns={columns}, rows={rows}, iconSize={iconSize}, maxContainerSize={maxContainerSize}");

        grid.cellSize = new Vector2(iconSize, iconSize);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        foreach (var order in pendingOrders)
        {
            if (orderImagePrefab == null) break;

            var obj = Instantiate(orderImagePrefab, orderImageContainer.transform);

            var objCanvas = obj.GetComponent<Canvas>();
            if (objCanvas != null) objCanvas.enabled = false;

            var img = obj.GetComponent<Image>();
            if (img != null && order.sushiData != null)
            {
                img.sprite = order.sushiData.sushiIcon;
                img.color = Color.white;
            }

            var le = obj.GetComponent<LayoutElement>();
            if (le == null) le = obj.AddComponent<LayoutElement>();
            le.preferredWidth = iconSize;
            le.preferredHeight = iconSize;
            le.minWidth = iconSize;
            le.minHeight = iconSize;

            orderImages.Add(img);
        }
    }
    private void DestroySelf() => Destroy(gameObject);
}