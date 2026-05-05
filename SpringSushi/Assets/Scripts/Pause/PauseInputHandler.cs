using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputHandler : MonoBehaviour
{
    // ここを Player に変更
    private Player inputActions;
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private void Awake()
    {
        // ここを Player に変更
        inputActions = new Player();
    }

    private void OnEnable()
    {
        // inputActions.Player ではなく .GamePlay に変更
        inputActions.GamePlay.Enable();
        inputActions.GamePlay.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        // ここも .GamePlay に変更
        inputActions.GamePlay.Pause.performed -= OnPausePerformed;
        inputActions.GamePlay.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (GameStateManager.Instance.IsPaused)
            pauseMenuUI.Resume();
        else
            pauseMenuUI.Pause();
    }
}