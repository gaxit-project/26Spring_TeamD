using UnityEngine;

public class TitleUI : MonoBehaviour
{
    public void Start()
    {
        SoundPlayer.Instance.PlayBGM(SoundKeys.BgmTitle);
    }

    public void OnStartButton()
    {
        Debug.Log($"[TitleUI] OnStartButton() ‰Ÿ‰º (GameState:{GameStateManager.Instance?.CurrentState})");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[TitleUI] š GameStateManager.Instance‚ªnull‚Å‚·I");
            return;
        }

        // Title ¨ Ready
        GameStateManager.Instance.EnterReady();
        StageManager.Instance.StartFirstStage();
    }

    public void OnStageSelectButton()
    {
        Debug.Log("[TitleUI] OnStageSelectButton()");
        SceneController.Instance.LoadSceneAsync("StageSelectScene");
    }

    public void OnExitButton() => Application.Quit();
}