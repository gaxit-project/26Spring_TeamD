using UnityEngine;
using TMPro; // TextMeshProを使う場合
// using UnityEngine.UI; // レガシーなTextを使う場合はこちらを有効化

public class ComboUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TMP_Text comboText;
    // [SerializeField] private Text comboText; // レガシーTextを使う場合はこちら

    [Header("表示設定")]
    [Tooltip("表示文字列のフォーマット（{0}がコンボ数に置き換わります）")]
    [SerializeField] private string textFormat = "{0} COMBOS!";

    [Tooltip("コンボがこの数値以上のときに表示する（例: 2にすると2コンボ目から表示）")]
    [SerializeField] private int minComboToShow = 1;

    private void Start()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += HandleComboChanged;
            // 起動時の初期反映
            UpdateDisplay(ComboManager.Instance.CurrentCombo);
        }
        else
        {
            UpdateDisplay(0);
        }
    }

    private void OnDestroy()
    {
        // イベント解除（メモリリーク防止）
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged -= HandleComboChanged;
        }
    }

    private void HandleComboChanged(int currentCombo, int feverProgress, int targetFever)
    {
        UpdateDisplay(currentCombo);
    }

    private void UpdateDisplay(int combo)
    {
        if (comboText == null) return;

        if (combo >= minComboToShow)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = string.Format(textFormat, combo);
        }
        else
        {
            // 0コンボ（失敗時など）はテキストを非表示にする
            comboText.gameObject.SetActive(false);
        }
    }
}