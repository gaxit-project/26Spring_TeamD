using UnityEngine;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ロード順（シーン名）")]
    [SerializeField] private List<string> stageOrder = new List<string>();

    [Header("各ステージのプレビュー画像（同じ順番で並べる）")]
    [SerializeField] private List<Sprite> stageSprites = new List<Sprite>();

    [Header("各ステージの売上目標データ（同じ順番で並べる）")]
    [SerializeField] private List<StageGoalData> stageGoals = new List<StageGoalData>();

    [Header("各ステージのゲームデータ（同じ順番で並べる）")]
    [SerializeField] private List<StageDataSO> stageDatas = new List<StageDataSO>();

    private int currentStageIndex = 0;
    private const string ClearKey = "ReachedStageIndex";

    /// <summary>現在ステージの売上目標データ</summary>
    public StageGoalData CurrentGoal { get; private set; }

    /// <summary>現在ステージのゲームデータ（客スポーン・寿司リスト等）</summary>
    public StageDataSO CurrentStageData { get; private set; }

    /// <summary>CurrentStageData を返すメソッド形式のアクセサ</summary>
    public StageDataSO GetCurrentStageData() => CurrentStageData;

    // =========================================================
    // Debug
    // =========================================================
    private bool debugForceUnlock = false;
    public bool IsDebugForceUnlock => debugForceUnlock;

    public void ToggleDebugUnlockAll()
    {
        debugForceUnlock = !debugForceUnlock;
        Debug.Log($"[DEBUG] Force Unlock = {debugForceUnlock}");
    }

    // =========================================================
    public int ReachedStageIndex
    {
        get => PlayerPrefs.GetInt(ClearKey, 0);
        private set { PlayerPrefs.SetInt(ClearKey, value); PlayerPrefs.Save(); }
    }

    public bool IsStageUnlocked(int index)
    {
        if (debugForceUnlock) return true;
        return index <= ReachedStageIndex;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (stageOrder.Count == 0)
            Debug.LogError("StageManager にステージが登録されていません");
        if (stageSprites.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageSprites の数が一致していません");
        if (stageGoals.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageGoals の数が一致していません");
        if (stageDatas.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageDatas の数が一致していません");
    }

    // =========================================================
    public Sprite GetStageSprite(int index)
    {
        if (index < 0 || index >= stageSprites.Count) return null;
        return stageSprites[index];
    }

    public int GetTotalStageCount() => stageOrder.Count;

    public string GetStageNameAt(int index)
        => (index >= 0 && index < stageOrder.Count) ? stageOrder[index] : "";

    public void StartFirstStage()
    {
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        currentStageIndex = 0;
        LoadCurrentStage();
    }

    public void RetryFromBeginning()
    {
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        currentStageIndex = 0;
        LoadCurrentStage();
    }

    public void RetryCurrentStage()
    {
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        LoadCurrentStage();
    }

    public void SelectStage(int index)
    {
        if (!IsStageUnlocked(index)) return;
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        currentStageIndex = index;
        LoadCurrentStage();
    }

    private void LoadCurrentStage()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stageOrder.Count) return;

        // 売上目標データをセット
        CurrentGoal = (currentStageIndex < stageGoals.Count)
            ? stageGoals[currentStageIndex] : null;

        if (CurrentGoal == null)
            Debug.LogWarning($"[StageManager] ステージ{currentStageIndex} の StageGoalData が未設定です。");

        // ゲームデータをセット
        CurrentStageData = (currentStageIndex < stageDatas.Count)
            ? stageDatas[currentStageIndex] : null;

        if (CurrentStageData == null)
            Debug.LogWarning($"[StageManager] ステージ{currentStageIndex} の StageDataSO が未設定です。");

        SceneController.Instance.LoadSceneAsync(stageOrder[currentStageIndex]);
    }
}