using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private Image previewImage;
    [SerializeField] private Button button;

    private int targetIndex;

    public void Setup(int index, string displayName)
    {
        targetIndex = index;

        if (button == null)
            button = GetComponent<Button>();

        stageText.text = displayName;
        previewImage.sprite = StageManager.Instance.GetStageSprite(index);
        previewImage.color = Color.white;

        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        Debug.Log($"Stage {targetIndex} Selected");
        GameStateManager.Instance.EnterReady();
        StageManager.Instance.SelectStage(targetIndex);
    }
}
/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private Image previewImage;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button button;

    private int targetIndex;
    private bool unlocked;

    public void Setup(int index, bool isUnlocked, string displayName)
    {
        targetIndex = index;
        unlocked = isUnlocked;

        if (button == null)
            button = GetComponent<Button>();

        stageText.text = isUnlocked ? displayName : "???";

        previewImage.sprite = StageManager.Instance.GetStageSprite(index);
        previewImage.color = isUnlocked ? Color.white : Color.gray;

        button.interactable = true;

        if (lockOverlay != null)
            lockOverlay.SetActive(!isUnlocked);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (unlocked)
        {
            Debug.Log($"Stage {targetIndex} Selected");
            StageManager.Instance.SelectStage(targetIndex);
        }
        else
        {
            ShowLockedMessage();
        }
    }

    private void ShowLockedMessage()
    {
        int needIndex = targetIndex - 1;
        string needName = StageManager.Instance.GetStageNameAt(needIndex);

        // TODO: UIポップアップでの通知が必要になったら、ここに実装する
        Debug.Log($"{needName} をクリアすると解放されます");
    }
}
*/