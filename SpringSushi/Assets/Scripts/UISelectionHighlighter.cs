using UnityEngine;
using UnityEngine.EventSystems; // 選択検知に必要

public class UISelectionHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject highlightPanel; // 文字の後ろに表示したいパネル

    private void Awake()
    {
        // 最初は非表示にしておく
        if (highlightPanel != null) highlightPanel.SetActive(false);
    }

    // フォーカス（選択）されたとき
    public void OnSelect(BaseEventData eventData)
    {
        if (highlightPanel != null) highlightPanel.SetActive(true);
    }

    // フォーカスが外れた（非選択）とき
    public void OnDeselect(BaseEventData eventData)
    {
        if (highlightPanel != null) highlightPanel.SetActive(false);
    }

    // パネルが非表示になったときも確実に消す処理
    private void OnDisable()
    {
        if (highlightPanel != null) highlightPanel.SetActive(false);
    }
}