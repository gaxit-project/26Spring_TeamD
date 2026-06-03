using UnityEngine;

public class StageGameInitializer : MonoBehaviour
{
    [SerializeField] private CustomerManager customerManager;

    private void Start()
    {
        StageDataSO stageData = StageManager.Instance?.GetCurrentStageData();

        if (stageData == null)
        {
            Debug.LogError("[StageGameInitializer] StageDataSO ‚ªæ“¾‚Å‚«‚Ü‚¹‚ñI");
            return;
        }

        customerManager.StartStage(stageData);
        // š StartPlaying() ‚Í‚±‚±‚Å‚ÍŒÄ‚Î‚È‚¢
        // GameStateManager.Instance?.StartPlaying(); © íœ
    }
}