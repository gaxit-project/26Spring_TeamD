using UnityEngine;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ロード順（シーン名）")]
    [SerializeField] private List<string> stageOrder = new List<string>();

    [Header("各ステージのプレビュー画像（同じ順番で並べる）")]
    [SerializeField] private List<Sprite> stageSprites = new List<Sprite>();

    [Header("各ステージのデータ（同じ順番で並べる）")]
    [SerializeField] private List<StageDataSO> stageDataList = new List<StageDataSO>();

    private int currentStageIndex = 0;
    private const string ClearKey = "ReachedStageIndex";

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
        private set
        {
            PlayerPrefs.SetInt(ClearKey, value);
            PlayerPrefs.Save();
        }
    }

    public bool IsStageUnlocked(int index)
    {
        if (debugForceUnlock) return true;
        return index <= ReachedStageIndex;
    }

    // =========================================================
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (stageOrder.Count == 0)
            Debug.LogError("StageManager にステージが登録されていません");
        if (stageSprites.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageSprites の数が一致していません！");
        if (stageDataList.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageDataList の数が一致していません！");
    }

    // =========================================================
    // 取得
    // =========================================================
    public int GetTotalStageCount() => stageOrder.Count;

    public string GetStageNameAt(int index) =>
        (index >= 0 && index < stageOrder.Count) ? stageOrder[index] : "";

    public Sprite GetStageSprite(int index) =>
        (index >= 0 && index < stageSprites.Count) ? stageSprites[index] : null;

    public StageDataSO GetStageData(int index) =>
        (index >= 0 && index < stageDataList.Count) ? stageDataList[index] : null;

    public StageDataSO GetCurrentStageData() => GetStageData(currentStageIndex);

    // =========================================================
    // 遷移
    // =========================================================
    public void StartFirstStage()
    {
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;
        currentStageIndex = 0;
        LoadCurrentStage();
    }

    public void RetryFromBeginning()
    {
        // 途切れていた箇所を修正し、処理を補完しました
        if (SceneController.Instance != null && SceneController.Instance.IsTransitioning) return;

        // 現在のステージを再読み込み
        LoadCurrentStage();
    }

    /// <summary>
    /// 現在のインデックスのステージをロードする
    /// </summary>
    private void LoadCurrentStage()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stageOrder.Count) return;

        Debug.Log($"[StageManager] SceneController.Instance = {SceneController.Instance}"); // ★追加

        SceneController.Instance.LoadSceneAsync(stageOrder[currentStageIndex]);
    }
}