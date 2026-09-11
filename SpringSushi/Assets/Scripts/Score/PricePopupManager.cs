using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;

/// <summary>
/// 寿司が届いたときのポップアップUIを管理する。
/// オブジェクトプールを使用してメモリ負荷（GC）を抑える。
/// </summary>
public class PricePopupManager : MonoBehaviour
{
    public static PricePopupManager Instance { get; private set; }

    [Header("参照")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Camera mainCamera;

    [Header("Popup Prefab")]
    [Tooltip("TextMeshProUGUIとCanvasGroupがついたPrefab")]
    [SerializeField] private GameObject popupPrefab;

    [Header("アニメーション設定")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float riseDistance = 50f;

    [Header("文字色設定")]
    [SerializeField] private Color positiveColor = Color.yellow;
    [SerializeField] private Color negativeColor = Color.red;
    [Tooltip("フィーバー時など強調表示したい時の文字色（未指定時はpositiveColor）")]
    [SerializeField] private Color feverColor = new Color(1f, 0.4f, 0.8f); // 鮮やかなマゼンタ/ピンク調

    // オブジェクトプールの定義
    private IObjectPool<GameObject> _pool;

    [Header("プール設定")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;

        // プールの初期化
        _pool = new ObjectPool<GameObject>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    #region プール用コールバック
    private GameObject CreatePooledItem()
    {
        return Instantiate(popupPrefab, hudCanvas.transform);
    }

    private void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }
    #endregion

    /// <summary>
    /// 通常の価格ポップアップを表示する（+100円 / -50円 など）
    /// </summary>
    public void ShowPopup(int price, Vector3 worldPosition)
    {
        string text = price >= 0 ? $"+{price}円" : $"{price}円";
        Color color = price >= 0 ? positiveColor : negativeColor;
        ShowPopupInternal(text, color, worldPosition);
    }

    /// <summary>
    /// ★ 追加: 任意の文字列を指定してポップアップを表示する（フィーバー時の「+100×2.0円」など）
    /// </summary>
    public void ShowPopup(string text, Vector3 worldPosition, Color? customColor = null)
    {
        // 色の指定がなければフィーバー用カラー（feverColor）を使用
        Color color = customColor ?? feverColor;
        ShowPopupInternal(text, color, worldPosition);
    }

    private void ShowPopupInternal(string text, Color color, Vector3 worldPosition)
    {
        if (popupPrefab == null || hudCanvas == null) return;

        // 3D座標 -> スクリーン座標 -> UIローカル座標
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPos.z < 0f) return;

        // プールから取得
        GameObject obj = _pool.Get();
        RectTransform rect = obj.GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hudCanvas.transform as RectTransform,
            screenPos,
            hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : hudCanvas.worldCamera,
            out localPoint
        );
        rect.localPosition = localPoint;

        // テキストと色の設定
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        // アニメーション開始
        StartCoroutine(AnimatePopup(obj, rect));
    }

    private IEnumerator AnimatePopup(GameObject obj, RectTransform rect)
    {
        if (!obj.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }

        Vector2 startPos = rect.localPosition;
        float elapsed = 0f;

        // 1. フェードイン
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 2. 維持
        yield return new WaitForSeconds(holdDuration);

        // 3. フェードアウト + 上昇
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = 1f - t;
            rect.localPosition = startPos + Vector2.up * (riseDistance * t);
            yield return null;
        }

        // 終了後にプールへ戻す
        _pool.Release(obj);
    }
}