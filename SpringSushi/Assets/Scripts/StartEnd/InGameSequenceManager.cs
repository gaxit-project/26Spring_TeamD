using UnityEngine;
using TMPro;
using System.Collections;

public class InGameSequenceManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DoorAnimation doorAnim;
    [SerializeField] private BusinessHoursTimer businessTimer;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Settings")]
    [SerializeField] private float operationTime = 60f; // 営業時間の長さ
    [SerializeField] private string resultSceneName = "ResultScene";

    private Player inputActions;
    private bool isSequenceStarted = false;

    private void Awake()
    {
        inputActions = new Player();
        // シーン開始時はゲームを止めておく
        GameStateManager.Instance.PauseGame();
        statusText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        // 入力があったらシーケンス開始
        inputActions.GamePlay.StartAction.performed += _ => StartBusinessSequence();
    }

    private void OnDisable()
    {
        inputActions.GamePlay.Disable();
    }

    private void StartBusinessSequence()
    {
        if (isSequenceStarted) return;
        isSequenceStarted = true;
        StartCoroutine(PlayBusinessSequenceCoroutine());
    }

    private IEnumerator PlayBusinessSequenceCoroutine()
    {
        // --- 1. 開店演出 (Start) ---
        statusText.text = "開店!!";
        statusText.gameObject.SetActive(true);

        yield return doorAnim.Open();

        statusText.gameObject.SetActive(false);

        // --- 2. 営業開始 (Playing) ---
        GameStateManager.Instance.ResumeGame();

        // 営業時間が終わるのを待つ
        yield return businessTimer.StartBusiness(operationTime);

        // --- 3. 閉店演出 (End) ---
        // ゲームを止めてから「終了」を表示
        GameStateManager.Instance.PauseGame();

        statusText.text = "閉店!!";
        statusText.gameObject.SetActive(true);

        // 扉を閉める
        yield return doorAnim.Close();

        // 余韻を持たせてからリザルトシーンへ
        yield return new WaitForSecondsRealtime(2f);
        SceneController.Instance.LoadSceneAsync(resultSceneName);
    }
}