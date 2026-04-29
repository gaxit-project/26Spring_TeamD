using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    public void Pause()
    {
        pauseUI.SetActive(true);
        GameStateManager.Instance.PauseGame();
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        GameStateManager.Instance.ResumeGame();
    }

    public void ReturnToTitle()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
        GameStateManager.Instance.ResumeGame();
    }

    public void Reset()
    {
        GameStateManager.Instance.ResumeGame();
        pauseUI.SetActive(false);

        StageRuntimeManager.EnsureExists().ResetStage();
    }
}