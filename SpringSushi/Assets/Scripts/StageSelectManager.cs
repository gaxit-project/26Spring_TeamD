using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StageSelectManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private StageSelectButton buttonPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Scroll設定")]
    [SerializeField] private bool smoothScroll = true;
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float buttonSize = 640;

    private bool generated = false;

    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GenerateButtons();
    }

    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        FollowSelectedButton();
    }

    // =========================================================
    // ボタン生成
    // =========================================================

    private void GenerateButtons()
    {
        if (buttonPrefab == null || container == null) return;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        int totalStages = StageManager.Instance.GetTotalStageCount();

        for (int i = 0; i < totalStages; i++)
        {
            StageSelectButton btn = Instantiate(buttonPrefab, container);

            string displayName = StageManager.Instance.GetStageDisplayNameAt(i);

            btn.Setup(i, displayName);
        }

        generated = true;
        SelectFirstButton();
    }

    // =========================================================
    // 初期選択
    // =========================================================

    private void SelectFirstButton()
    {
        if (container.childCount == 0) return;

        Button first = container.GetChild(0).GetComponent<Button>();
        EventSystem.current.SetSelectedGameObject(first.gameObject);
    }

    // =========================================================
    // スクロール追従
    // =========================================================

    private void FollowSelectedButton()
    {
        if (!generated || scrollRect == null) return;
        if (EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        if (selected.transform.parent != container) return;

        RectTransform target = selected.GetComponent<RectTransform>();
        if (target == null) return;

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        if (content == null || viewport == null) return;

        if (content.rect.width <= viewport.rect.width) return;

        float targetPosX = target.anchoredPosition.x;

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;
        float scrollRange = contentWidth - viewportWidth;

        float normalized = (targetPosX + buttonSize - (viewportWidth * 0.5f)) / scrollRange;
        normalized = Mathf.Clamp01(normalized);

        if (smoothScroll)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                normalized,
                scrollSpeed * Time.unscaledDeltaTime
            );
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = normalized;
        }
    }

    // =========================================================

    public void OnClickBackButton()
    {
        SceneController.Instance.LoadSceneAsync("Title");
    }
}