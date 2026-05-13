using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputHandler : MonoBehaviour
{
    private Player inputActions;
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private void Awake()
    {
        inputActions = new Player();
    }

    private void OnEnable()
    {
        inputActions.GamePlay.Enable();
        inputActions.GamePlay.Pause.performed += OnPausePerformed;
        Debug.Log("[PauseInput] OnEnable: Pause“ü—Íw“ÇŠ®—¹");
    }

    private void OnDisable()
    {
        inputActions.GamePlay.Pause.performed -= OnPausePerformed;
        inputActions.GamePlay.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        var state = GameStateManager.Instance?.CurrentState;
        Debug.Log($"[PauseInput] Pause“ü—ÍŒŸ’m (GameState:{state})");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[PauseInput] š GameStateManager.Instance‚ªnull‚Å‚·I");
            return;
        }

        if (GameStateManager.Instance.IsReady)
        {
            Debug.LogWarning("[PauseInput] š Ready’†‚Ì‚½‚ßPause“ü—Í‚ğ–³‹");
            return;
        }

        if (GameStateManager.Instance.IsPaused)
            pauseMenuUI.Resume();
        else
            pauseMenuUI.Pause();
    }
}