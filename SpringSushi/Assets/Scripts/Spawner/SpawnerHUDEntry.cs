using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 1つのSushiSpawnerに対応する「? [寿司画像] ?」UI。
/// SpawnerHUDから生成・管理される。
/// </summary>
public class SpawnerHUDEntry : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Image sushiIcon;
    [SerializeField] private GameObject cursorFrame; // 選択中のハイライト枠

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
    }

    private void LateUpdate()
    {
        if (spawner == null) return;
        FollowSpawner();
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
                out Vector2 local
            );
            root.localPosition = local;
        }
    }

    /// <summary>選択中の寿司アイコンを更新する</summary>
    public void UpdateIcon()
    {
        if (spawner == null || sushiIcon == null) return;
        var sushi = spawner.SelectedSushi;
        sushiIcon.sprite = sushi != null ? sushi.sushiIcon : null;
    }

    /// <summary>このSpawnerが選択中かどうかを反映する</summary>
    public void SetSelected(bool selected)
    {
        if (cursorFrame != null)
            cursorFrame.SetActive(selected);
    }
}