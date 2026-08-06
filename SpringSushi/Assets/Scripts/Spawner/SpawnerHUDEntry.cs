using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Image sushiIcon;
    [SerializeField] private GameObject cursorFrame;

    [Header("左右プレビューアイコン")]
    [Tooltip("Arrows/Left上に配置した、1つ前の寿司を表示するImage")]
    [SerializeField] private Image leftPreviewIcon;
    [Tooltip("Arrows/Right上に配置した、1つ次の寿司を表示するImage")]
    [SerializeField] private Image rightPreviewIcon;

    [Header("クールダウンタイマー")]
    [SerializeField] private Image cooldownCircle;
    [SerializeField] private TextMeshProUGUI cooldownText;

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

        if (cooldownCircle != null)
        {
            cooldownCircle.gameObject.SetActive(isOnCD);
            cooldownCircle.fillAmount = rate;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isOnCD);
            cooldownText.text = remaining.ToString("F1");
        }
    }

    /// <summary>
    /// 選択中の寿司アイコンに加え、左右矢印の上に前後の寿司アイコンを表示する。
    /// </summary>
    public void UpdateIcon()
    {
        if (spawner == null) return;

        if (sushiIcon != null)
            sushiIcon.sprite = spawner.SelectedSushi?.sushiIcon;

        UpdateAdjacentIcons();
    }

    private void UpdateAdjacentIcons()
    {
        var list = spawner.sushiDataList;
        if (list == null || list.Count == 0) return;

        int count = list.Count;
        int current = spawner.SelectedIndex;

        // (x-1)。x=0のときは末尾(n-1)へループする
        int prevIndex = (current - 1 + count) % count;
        // (x+1)。末尾のときは先頭(0)へループする
        int nextIndex = (current + 1) % count;

        if (leftPreviewIcon != null)
            leftPreviewIcon.sprite = list[prevIndex]?.sushiIcon;

        if (rightPreviewIcon != null)
            rightPreviewIcon.sprite = list[nextIndex]?.sushiIcon;
    }

    public void SetSelected(bool selected)
    {
        if (cursorFrame != null)
            cursorFrame.SetActive(selected);
    }
}