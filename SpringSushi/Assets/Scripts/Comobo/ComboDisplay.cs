using TMPro;
using UnityEngine;

/// <summary>
/// ComboCanvasに置く。コンボ数を画面端に常時表示。
/// コンボ0のときは非表示。
/// </summary>
public class ComboDisplay : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject comboRoot;   // 表示/非表示を切り替えるルート
    [SerializeField] private TextMeshProUGUI comboText;

    private void Start()
    {
        // 初期状態は非表示
        comboRoot.SetActive(false);

        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboChanged += UpdateDisplay;
    }

    private void OnDestroy()
    {
        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int count)
    {
        if (count <= 0)
        {
            comboRoot.SetActive(false);
            return;
        }

        comboRoot.SetActive(true);
        comboText.text = $"{count}\nコンボ";
    }
}