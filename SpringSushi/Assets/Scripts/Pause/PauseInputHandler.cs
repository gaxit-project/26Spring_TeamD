using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputHandler : MonoBehaviour
{
    private Player inputActions;

    [SerializeField] private PauseMenuUI pauseMenuUI;

    // ★ 連打デバウンス: 前回の入力から最低この秒数が経過するまで無視
    [SerializeField] private float pauseInputCooldown = 0.2f;
    private float lastPauseInputTime = -999f;

    private void Awake()
    {
        inputActions = new Player();
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        inputActions.GamePlay.Pause.performed += OnPausePerformed;
        Debug.Log("[PauseInput] OnEnable: Pause入力購読完了");
    }

    private void OnDisable()
    {
        // ★ メソッド参照で登録・解除を統一（ラムダは使わない）
        inputActions.GamePlay.Pause.performed -= OnPausePerformed;
        inputActions.GamePlay.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // ★ クールダウン中は無視（連打デバウンス）
        if (Time.unscaledTime - lastPauseInputTime < pauseInputCooldown)
        {
            Debug.LogWarning("[PauseInput] Pause入力をクールダウン中のため無視");
            return;
        }
        lastPauseInputTime = Time.unscaledTime;

        var state = GameStateManager.Instance?.CurrentState;
        Debug.Log($"[PauseInput] Pause入力検知 (GameState:{state})");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[PauseInput] ★ GameStateManager.Instanceがnullです！");
            return;
        }

        if (GameStateManager.Instance.IsReady)
        {
            Debug.LogWarning("[PauseInput] ★ Ready中のためPause入力を無視");
            return;
        }

        // ★ 遷移中はPause操作を受け付けない
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning)
        {
            Debug.LogWarning("[PauseInput] ★ シーン遷移中のためPause入力を無視");
            return;
        }

        if (GameStateManager.Instance.IsPaused)
            pauseMenuUI.Resume();
        else
            pauseMenuUI.Pause();
    }
}