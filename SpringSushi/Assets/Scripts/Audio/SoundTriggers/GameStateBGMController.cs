using UnityEngine;

/// <summary>
/// GameStateManagerの状態変化を監視し、自動的にBGMを切り替えるスクリプト
/// AudioManager（またはGameStateManager）と同じGameObjectにアタッチして使います。
/// </summary>
public class GameStateBgmController : MonoBehaviour
{
    private void Start()
    {
        if (GameStateManager.Instance == null) return;

        // イベントに登録
        GameStateManager.Instance.OnStateChanged += HandleStateChanged;

        // 起動時の初期Stateに合わせてBGMを処理
        PlayBgmForState(GameStateManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameStateManager.GameState prev, GameStateManager.GameState current)
    {
        PlayBgmForState(current);
    }

    private void PlayBgmForState(GameStateManager.GameState state)
    {
        if (SoundPlayer.Instance == null) return;

        switch (state)
        {
            case GameStateManager.GameState.Title:
                SoundPlayer.Instance.PlayBGM(SoundKeys.BgmTitle);
                break;

            case GameStateManager.GameState.Ready:
                // ★修正: Ready中（「位置について」などの待機時間）は一旦BGMを止める
                SoundPlayer.Instance.StopBGM();
                break;

            case GameStateManager.GameState.Playing:
                SoundPlayer.Instance.PlayBGM(SoundKeys.BgmGame);
                break;

            case GameStateManager.GameState.Paused:
                // ポーズ中はそのまま前のBGMを鳴らし続けるため、何もしない
                // （もしポーズ中も止めたい場合は StopBGM() に変更してください）
                break;

            case GameStateManager.GameState.Result:
                SoundPlayer.Instance.PlayBGM(SoundKeys.BgmResult);
                break;
        }
    }
}