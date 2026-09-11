using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 設定画面のUI管理。SoundManagerの音量をスライダーで操作する。
/// </summary>
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
    public Slider voiceSlider;

    [Header("First Selected")]
    public GameObject audioTabButton;
    public GameObject startButton;
    public GameObject backToMainButton; // 戻るボタン

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        masterSlider.value = SoundManager.Instance.masterVolume;
        bgmSlider.value = SoundManager.Instance.bgmVolume;
        sfxSlider.value = SoundManager.Instance.sfxVolume;
        voiceSlider.value = SoundManager.Instance.voiceVolume;

        masterSlider.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSfxVolume);
        voiceSlider.onValueChanged.AddListener(SoundManager.Instance.SetVoiceVolume);

        // ▼▼ ここを追加：ボタンの横移動を無効化（縦移動のみ許可） ▼▼
        SetVerticalNavigationOnly(audioTabButton);
        SetVerticalNavigationOnly(backToMainButton);
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        ShowMainMenu();
    }

    // ★ ナビゲーションを縦のみ（Vertical）に固定するメソッド
    private void SetVerticalNavigationOnly(GameObject buttonObj)
    {
        if (buttonObj == null) return;

        Button btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            Navigation nav = btn.navigation;
            nav.mode = Navigation.Mode.Vertical; // 左右入力を無視し、上下遷移のみにする
            btn.navigation = nav;
        }
    }

    private void Update()
    {
        // 設定パネルが開いていない時は処理しない
        if (settingsPanel == null || !settingsPanel.activeSelf) return;

        // ゲームパッドのBボタンが押された時のみ判定
        if (Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame)
        {
            // スライダーを選択中なら、audioTabButton へフォーカスを戻す
            if (IsAnySliderSelected())
            {
                SetFocus(audioTabButton);
            }
        }
    }

    /// <summary>
    /// 現在いずれかの音量スライダーを選択中かどうか判定
    /// </summary>
    private bool IsAnySliderSelected()
    {
        if (EventSystem.current == null) return false;

        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return false;

        return current == masterSlider.gameObject ||
               current == bgmSlider.gameObject ||
               current == sfxSlider.gameObject ||
               current == voiceSlider.gameObject;
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsPanel.SetActive(true);
        audioPanel.SetActive(true);
        SetFocus(audioTabButton);
    }

    public void ShowAudioSettings()
    {
        audioPanel.SetActive(true);
        SetFocus(masterSlider.gameObject);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsPanel.SetActive(false);
        SetFocus(startButton);
    }

    private void SetFocus(GameObject target)
    {
        if (target != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}