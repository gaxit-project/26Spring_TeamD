using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject settingsPanel;
    public GameObject audioPanel;

    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("First Selected Buttons")]
    public GameObject graphicsTabButton; // UIだけ残す（押せない）
    public GameObject audioTabButton;
    public GameObject backToMainButton;
    public GameObject startButton;

    void Start()
    {
        if (SoundManager.Instance == null) return;

        // ===== スライダー初期値 =====
        masterSlider.value = SoundManager.Instance.masterVolume;
        bgmSlider.value = SoundManager.Instance.bgmVolume;
        sfxSlider.value = SoundManager.Instance.environmentVolume;

        // ===== リスナー登録 =====
        masterSlider.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetEnvVolume);

        // ===== Graphicsタブ封印（UIは残す）=====
        DisableGraphicsTab();

        ShowMainMenu();
    }

    // =========================
    // Settingsを開く
    // =========================
    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsPanel.SetActive(true);

        // Audioをデフォルト表示
        audioPanel.SetActive(true);

        DisableGraphicsTab();

        SetFocus(audioTabButton);
    }

    // =========================
    // Audio設定表示
    // =========================
    public void ShowAudioSettings()
    {
        audioPanel.SetActive(true);

        // Audio開いた瞬間 Master を選択
        SetFocus(masterSlider.gameObject);
    }

    // =========================
    // Graphics（未実装ダミー）
    // =========================
    public void ShowGraphicsSettings()
    {
        // 未実装
        return;
    }

    // =========================
    // メインメニューに戻る
    // =========================
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsPanel.SetActive(false);

        SetFocus(startButton);
    }

    // =========================
    // Graphicsタブを無効化
    // =========================
    private void DisableGraphicsTab()
    {
        if (graphicsTabButton == null) return;

        Button btn = graphicsTabButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = false;
        }
    }

    // =========================
    // フォーカス設定
    // =========================
    private void SetFocus(GameObject target)
    {
        if (target != null)
        {
            EventSystem.current.SetSelectedGameObject(target);
        }
    }
}
