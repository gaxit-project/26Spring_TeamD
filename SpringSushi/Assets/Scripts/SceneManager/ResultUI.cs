using UnityEngine;

public class ResultUI : MonoBehaviour
{
    public void Retry()
    {
        StageManager.Instance.RetryFromBeginning();
    }

    public void OnReturnToTitle()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Title);
        SceneController.Instance.LoadSceneAsync("Title");
    }
}