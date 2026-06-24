using UnityEngine;
using TMPro;
using System.Collections;

public class InGameSequenceManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DoorAnimation doorAnim;
    [SerializeField] private BusinessHoursTimer businessTimer;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("売上目標UI（StartEnd_Canvas上）")]
    [SerializeField] private StageGoalUI stageGoalUI;

    [Header("Settings")]
    [SerializeField] private float operationTime = 60f; // ← StageDataSOで上書きされる
    [SerializeField] private string resultSceneName = "ResultScene";
    [SerializeField] private StageDataSO stageData; // ★ 追加

    private Player inputActions;
    private bool isSequenceStarted = false;

    private void Awake()
    {
        inputActions = new Player();
        statusText.gameObject.SetActive(false);
        Time.timeScale = 0f;

        // ★ StageDataSOからoperationTimeを上書き
        if (stageData != null)
            operationTime = stageData.operationTime;

        Debug.Log($"[Sequence] Awake: timeScale=0 (GameState:{GameStateManager.Instance?.CurrentState})");
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        inputActions.GamePlay.StartAction.performed += OnStartActionPerformed;
    }

    private void OnDisable()
    {
        inputActions.GamePlay.StartAction.performed -= OnStartActionPerformed;
        inputActions.GamePlay.Disable();
    }

    private void OnStartActionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryStartSequence();
    }

    private void TryStartSequence()
    {
        if (GameStateManager.Instance == null) return;
        if (!GameStateManager.Instance.IsReady) return;
        if (isSequenceStarted) return;

        // 売上目標UIの表示が終わるまでスタートを受け付けない
        if (stageGoalUI != null && !stageGoalUI.IsReadyToStart)
        {
            Debug.Log("[Sequence] 売上目標表示中のためスタートを待機");
            return;
        }

        isSequenceStarted = true;
        inputActions.GamePlay.StartAction.performed -= OnStartActionPerformed;
        inputActions.GamePlay.Disable();

        StartCoroutine(PlayBusinessSequenceCoroutine());
    }

    private IEnumerator PlayBusinessSequenceCoroutine()
    {
        // --- 1. 開店演出 ---
        Debug.Log("[Sequence] 開店演出開始");

        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.OpenStore);
            SoundPlayer.Instance.PlaySFX(SoundKeys.DoorOpen);
        }

        statusText.text = "開店!!";
        statusText.gameObject.SetActive(true);
        // ★ ドアが開ききったタイミングでGoalPanelを非表示にする
        stageGoalUI?.HidePanel();

        yield return doorAnim.Open();

        statusText.gameObject.SetActive(false);

        // --- 2. ドア開放完了 → ゲーム開始 ---
        Debug.Log("[Sequence] ドア開放完了 → StartPlaying()");
        Time.timeScale = 1f;
        GameStateManager.Instance.StartPlaying();

        // ★ 追加：ここで全SushiSpawnerの生成を許可する
        var spawners = FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
            spawner.SetSpawningEnabled(true);

        yield return businessTimer.StartBusiness(operationTime);
        // --- 3. 閉店演出 ---
        Debug.Log("[Sequence] 営業終了 → 閉店演出");
        GameStateManager.Instance.PauseGame();

        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlaySFX(SoundKeys.CloseStore);
            SoundPlayer.Instance.PlaySFX(SoundKeys.DoorOpen);
        }

        statusText.text = "閉店!!";
        statusText.gameObject.SetActive(true);

        yield return doorAnim.Close();

        yield return new WaitForSecondsRealtime(2f);

        // --- 4. Result 遷移 ---
        Debug.Log("[Sequence] Result遷移");
        GameStateManager.Instance.EnterResult();
        SceneController.Instance.LoadSceneAsync(resultSceneName);
    }
}