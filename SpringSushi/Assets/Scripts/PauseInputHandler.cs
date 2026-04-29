using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [SerializeField] private PauseMenuUI pauseMenuUI; // UI§Œä‚ğ’S“–‚·‚éƒNƒ‰ƒX‚ğQÆ

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Pause.performed -= OnPausePerformed;
        inputActions.Player.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (GameStateManager.Instance.IsPaused)
            pauseMenuUI.Resume();
        else
            pauseMenuUI.Pause();
    }
}