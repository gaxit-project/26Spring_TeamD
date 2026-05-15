using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

        ShowMainMenu();
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