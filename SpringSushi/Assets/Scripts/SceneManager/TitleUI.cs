using UnityEngine;

public class TitleUI : MonoBehaviour
{
    public void OnStartButton()
    {
        GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        StageManager.Instance.StartFirstStage();
    }

    public void OnStageSelectButton()
    {
        SceneController.Instance.LoadSceneAsync("StageSelectScene");
    }

    public void OnExitButton() => Application.Quit();
}
