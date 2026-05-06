using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 寿司が届いたときに+price のUIをHUD_Canvas上でフェードイン→上へフェードアウトさせる。
///
/// 【セットアップ】
/// HUD_Canvas上の空GameObjectに本スクリプトをアタッチ。
/// popupPrefab には TextMeshProUGUI がついたPrefabをアサイン。
/// </summary>
public class PricePopupManager : MonoBehaviour
{
    public static PricePopupManager Instance { get; private set; }

    [Header("参照")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Camera mainCamera;

    [Header("Popup Prefab")]
    [Tooltip("TextMeshProUGUIがついたPrefab。テキストに価格が入る")]
    [SerializeField] private GameObject popupPrefab;

    [Header("アニメーション設定")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float riseDistance = 60f;
    [SerializeField] private Color positiveColor = Color.yellow;
    [SerializeField] private Color negativeColor = Color.red;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;
    }

    /// <summary>
    /// worldPosition（寿司のワールド座標）の位置にポップアップを表示する。
    /// </summary>
    public void ShowPopup(int price, Vector3 worldPosition)
    {
        if (popupPrefab == null || hudCanvas == null) return;

        // WorldPos → Screen → HUDLocal
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPos.z < 0f) return; // カメラ後方は非表示

        GameObject obj = Instantiate(popupPrefab, hudCanvas.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();

        // 座標変換
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hudCanvas.transform as RectTransform,
            screenPos,
            hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : hudCanvas.worldCamera,
            out localPoint
        );
        rect.localPosition = localPoint;

        // テキスト設定
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = price >= 0 ? $"+{price}円" : $"{price}円";
            tmp.color = price >= 0 ? positiveColor : negativeColor;
        }

        StartCoroutine(AnimatePopup(obj, rect, tmp));
    }

    private IEnumerator AnimatePopup(GameObject obj, RectTransform rect, TextMeshProUGUI tmp)
    {
        Vector2 startPos = rect.localPosition;
        Color baseColor = tmp != null ? tmp.color : Color.white;

        // フェードイン
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeInDuration);
            if (tmp != null) tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        // ホールド
        yield return new WaitForSeconds(holdDuration);

        // フェードアウト（上へ移動しながら）
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / fadeOutDuration);
            float alpha = 1f - ratio;
            if (tmp != null) tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            rect.localPosition = startPos + Vector2.up * (riseDistance * ratio);
            yield return null;
        }

        Destroy(obj);
    }
}