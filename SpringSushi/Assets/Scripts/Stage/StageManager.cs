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

    /// <summary>現在ステージの売上目標データ</summary>
    public StageGoalData CurrentGoal { get; private set; }

    /// <summary>現在ステージのゲームデータ（客スポーン・寿司リスト等）</summary>
    public StageDataSO CurrentStageData { get; private set; }

    /// <summary>CurrentStageData を返すメソッド形式のアクセサ</summary>
    public StageDataSO GetCurrentStageData() => CurrentStageData;

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

    /// <summary>シーン名(ロード先)を返す。SceneController.LoadSceneAsyncに渡す用。</summary>
    public string GetStageNameAt(int index)
        => (index >= 0 && index < stageOrder.Count) ? stageOrder[index] : "";

    /// <summary>
    /// プレイヤーに表示するステージ名を返す。StageDataSO.stageNameを参照する。
    /// StageDataSOが未設定、またはstageNameが空の場合は、シーン名(GetStageNameAt)にフォールバックする。
    /// </summary>
    public string GetStageDisplayNameAt(int index)
    {
        if (index >= 0 && index < stageDatas.Count)
        {
            var data = stageDatas[index];
            if (data != null && !string.IsNullOrEmpty(data.stageName))
                return data.stageName;
        }

        return GetStageNameAt(index);
    }

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