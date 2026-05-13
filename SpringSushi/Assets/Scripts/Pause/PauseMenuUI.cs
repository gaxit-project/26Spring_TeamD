using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    public void Pause()
    {
        var state = GameStateManager.Instance?.CurrentState;
        Debug.Log($"[PauseMenuUI] Pause() ŒÄ‚Ño‚µ (GameState:{state})");

        if (GameStateManager.Instance.IsReady)
        {
            Debug.LogWarning("[PauseMenuUI] š Ready’†‚Ì‚½‚ßPause‹‘”Û");
            return;
        }

        pauseUI.SetActive(true);
        GameStateManager.Instance.PauseGame();
    }

    public void Resume()
    {
        Debug.Log("[PauseMenuUI] Resume() ŒÄ‚Ño‚µ");
        pauseUI.SetActive(false);
        GameStateManager.Instance.ResumeGame();
    }

    public void ReturnToTitle()
    {
        Debug.Log("[PauseMenuUI] ReturnToTitle() ŒÄ‚Ño‚µ");
        GameStateManager.Instance.ResumeGame();
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
    }
}