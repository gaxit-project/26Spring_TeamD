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
        Time.timeScale = 0f;
        Debug.Log($"[Sequence] Awake: timeScale=0 (GameState:{GameStateManager.Instance?.CurrentState})");
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        // ラムダではなくメソッド参照を使うことで、OnDisable で確実に解除できる
        inputActions.GamePlay.StartAction.performed += OnStartActionPerformed;
    }

    private void OnDisable()
    {
        // ラムダだと別インスタンス扱いになり解除されないため、メソッド参照で統一
        inputActions.GamePlay.StartAction.performed -= OnStartActionPerformed;
        inputActions.GamePlay.Disable();
    }

    // InputSystem のコールバックを名前付きメソッドで受ける
    private void OnStartActionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryStartSequence();
    }

    private void TryStartSequence()
    {
        if (GameStateManager.Instance == null) return;
        if (!GameStateManager.Instance.IsReady) return;

        // 連打ガード: 一度でも開始したら以降の入力を完全無視
        if (isSequenceStarted) return;
        isSequenceStarted = true;

        // 入力自体をここで無効化し、コールバック到達を物理的に止める
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

        yield return doorAnim.Open();

        statusText.gameObject.SetActive(false);

        // --- 2. ドア開放完了 → ゲーム開始 ---
        Debug.Log("[Sequence] ドア開放完了 → StartPlaying()");
        Time.timeScale = 1f;
        GameStateManager.Instance.StartPlaying();

        yield return businessTimer.StartBusiness(operationTime);

        // --- 3. 閉店演出 ---
        Debug.Log("[Sequence] 営業終了 → 閉店演出");
        GameStateManager.Instance.PauseGame(); // timeScale=0

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