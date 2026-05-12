using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Pool; // Unity標準のプール機能を使用

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
    [SerializeField] private Color positiveColor = Color.yellow;
    [SerializeField] private Color negativeColor = Color.red;

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
            createFunc: CreatePooledItem,      // 新しく作る時
            actionOnGet: OnTakeFromPool,      // プールから出す時
            actionOnRelease: OnReturnedToPool, // プールに戻す時
            actionOnDestroy: OnDestroyPoolObject, // 最大サイズを超えて破棄される時
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
    /// 指定のワールド座標に価格ポップアップを表示する
    /// </summary>
    public void ShowPopup(int price, Vector3 worldPosition)
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
            tmp.text = price >= 0 ? $"+{price}円" : $"{price}円";
            tmp.color = price >= 0 ? positiveColor : negativeColor;
        }

        // アニメーション開始
        StartCoroutine(AnimatePopup(obj, rect));
    }

    private IEnumerator AnimatePopup(GameObject obj, RectTransform rect)
    {
        // CanvasGroupを使って一括フェードさせる
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