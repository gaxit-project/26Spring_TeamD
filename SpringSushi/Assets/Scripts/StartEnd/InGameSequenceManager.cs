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
    [SerializeField] private float operationTime = 60f;
    [SerializeField] private string resultSceneName = "ResultScene";

    private Player inputActions;
    private bool isSequenceStarted = false;

    private void Awake()
    {
        inputActions = new Player();
        statusText.gameObject.SetActive(false);

        // シーン開始時にゲームを止める（ドア演出はunscaledTimeで動くので問題なし）
        // StateはReady（TitleUIで設定済み）のままにする
        // PauseGame()はPlaying中にしか動かないため、ここではtimeScaleだけ止める
        Time.timeScale = 0f;
        Debug.Log($"[Sequence] Awake: timeScale=0 (GameState:{GameStateManager.Instance?.CurrentState})");
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        inputActions.GamePlay.StartAction.performed += _ => TryStartSequence();
        Debug.Log("[Sequence] OnEnable: StartAction購読完了");
    }

    private void OnDisable()
    {
        inputActions.GamePlay.StartAction.performed -= _ => TryStartSequence();
        inputActions.GamePlay.Disable();
    }

    private void TryStartSequence()
    {
        var state = GameStateManager.Instance?.CurrentState;
        Debug.Log($"[Sequence] StartAction入力 (GameState:{state}, isSequenceStarted:{isSequenceStarted})");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[Sequence] ★ GameStateManager.Instanceがnull");
            return;
        }

        // Ready状態のときだけ受け付ける
        if (!GameStateManager.Instance.IsReady)
        {
            Debug.LogWarning($"[Sequence] ★ Ready状態でないため開始不可 (現在:{state})");
            return;
        }

        if (isSequenceStarted)
        {
            Debug.Log("[Sequence] すでに開始済みのためスキップ");
            return;
        }

        isSequenceStarted = true;
        StartCoroutine(PlayBusinessSequenceCoroutine());
    }

    private IEnumerator PlayBusinessSequenceCoroutine()
    {
        // --- 1. 開店演出（timeScale=0のままunscaledTimeで動く） ---
        Debug.Log("[Sequence] 開店演出開始");
        statusText.text = "開店!!";
        statusText.gameObject.SetActive(true);
        yield return doorAnim.Open();
        statusText.gameObject.SetActive(false);

        // --- 2. ドアが開ききったらPlaying開始（ここで初めてPause可能） ---
        Debug.Log("[Sequence] ドア開放完了 → StartPlaying()");
        Time.timeScale = 1f;
        GameStateManager.Instance.StartPlaying();

        // 営業時間が終わるのを待つ
        yield return businessTimer.StartBusiness(operationTime);

        // --- 3. 閉店演出 ---
        Debug.Log("[Sequence] 営業終了 → 閉店演出");
        GameStateManager.Instance.PauseGame(); // timeScale=0
        statusText.text = "閉店!!";
        statusText.gameObject.SetActive(true);
        yield return doorAnim.Close();

        yield return new WaitForSecondsRealtime(2f);

        // --- 4. Result ---
        Debug.Log("[Sequence] Result遷移");
        GameStateManager.Instance.EnterResult();
        SceneController.Instance.LoadSceneAsync(resultSceneName);
    }
}