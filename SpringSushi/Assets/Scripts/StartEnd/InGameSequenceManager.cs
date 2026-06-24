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
    [SerializeField] private float operationTime = 60f;
    [SerializeField] private string resultSceneName = "ResultScene";
    [SerializeField] private StageDataSO stageData;

    private Player inputActions;
    private bool isSequenceStarted = false;

    // ★ 追加：客が全員帰ったかどうかのフラグ
    private bool allCustomersExited = false;

    private void Awake()
    {
        inputActions = new Player();
        statusText.gameObject.SetActive(false);
        Time.timeScale = 0f;

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

        // ★ 念のため購読解除（コルーチン中断時の解除漏れ防止）
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnAllCustomersExited -= HandleAllCustomersExited;
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

    // ★ 追加：CustomerManagerからの通知を受けるハンドラー
    private void HandleAllCustomersExited()
    {
        allCustomersExited = true;
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
        stageGoalUI?.HidePanel();

        yield return doorAnim.Open();

        statusText.gameObject.SetActive(false);

        // --- 2. ドア開放完了 → ゲーム開始 ---
        Debug.Log("[Sequence] ドア開放完了 → StartPlaying()");
        Time.timeScale = 1f;
        GameStateManager.Instance.StartPlaying();

        var spawners = FindObjectsByType<SushiSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
            spawner.SetSpawningEnabled(true);

        // ★ 変更：営業時間タイマーと「客が全員帰った」を並行監視し、早い方で終了する
        allCustomersExited = false;
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnAllCustomersExited += HandleAllCustomersExited;

        Coroutine businessRoutine = StartCoroutine(businessTimer.StartBusiness(operationTime));

        while (businessTimer.IsInBusiness && !allCustomersExited)
            yield return null;

        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnAllCustomersExited -= HandleAllCustomersExited;

        // ★ 客が全員帰って早期終了した場合、タイマーCoroutineを止めてUIを100%にする
        if (businessTimer.IsInBusiness)
        {
            StopCoroutine(businessRoutine);
            businessTimer.ForceComplete();
            Debug.Log("[Sequence] 全客退店により営業時間タイマーを早期終了");
        }

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