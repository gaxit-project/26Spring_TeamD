using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Image sushiIcon;
    [SerializeField] private GameObject cursorFrame;

    [Header("クールダウンタイマー")]
    [SerializeField] private Image cooldownCircle;
    [SerializeField] private TextMeshProUGUI cooldownText; // ★ 追加

    private SushiSpawner spawner;
    private Camera mainCamera;
    private Canvas hudCanvas;

    public void Initialize(SushiSpawner target, Canvas canvas, Camera cam)
    {
        spawner = target;
        hudCanvas = canvas;
        mainCamera = cam;
        UpdateIcon();
        SetSelected(false);

        if (cooldownCircle != null)
            cooldownCircle.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (spawner == null) return;
        FollowSpawner();
        UpdateCooldown();
    }

    private void FollowSpawner()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(spawner.WorldPosition + Vector3.up * 0.5f);
        if (screenPos.z < 0f) { root.gameObject.SetActive(false); return; }
        root.gameObject.SetActive(true);

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
                out Vector2 local);
            root.localPosition = local;
        }
    }

    private void UpdateCooldown()
    {
        float rate = spawner.GetCooldownRate();
        float remaining = spawner.GetCooldown(spawner.SelectedSushi);
        bool isOnCD = rate > 0f;

        // --- サークル ---
        if (cooldownCircle != null)
        {
            cooldownCircle.gameObject.SetActive(isOnCD);
            cooldownCircle.fillAmount = rate;
        }

        // --- テキスト ---
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isOnCD);
            // 小数点1桁で表示（例: 3.4）
            cooldownText.text = remaining.ToString("F1");
        }
    }

    public void UpdateIcon()
    {
        if (spawner == null || sushiIcon == null) return;
        sushiIcon.sprite = spawner.SelectedSushi?.sushiIcon;
    }

    public void SetSelected(bool selected)
    {
        if (cursorFrame != null)
            cursorFrame.SetActive(selected);
    }
}