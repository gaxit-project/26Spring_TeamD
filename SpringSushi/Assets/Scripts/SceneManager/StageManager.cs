using UnityEngine;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ロード順（シーン名）")]
    [SerializeField] private List<string> stageOrder = new List<string>();

    [Header("各ステージのプレビュー画像（同じ順番で並べる）")]
    [SerializeField] private List<Sprite> stageSprites = new List<Sprite>();

    private int currentStageIndex = 0;

    private const string ClearKey = "ReachedStageIndex";

    // =========================================================
    // ★ Debug（追加）
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

    // =========================================================
    // ★ 解放判定を一元化（超重要）
    // =========================================================
    public bool IsStageUnlocked(int index)
    {
        if (debugForceUnlock)
            return true;

        return index <= ReachedStageIndex;
    }

    // =========================================================

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (stageOrder.Count == 0)
            Debug.LogError("StageManager にステージが登録されていません");

        if (stageSprites.Count != stageOrder.Count)
            Debug.LogWarning("stageOrder と stageSprites の数が一致していません！");
    }

    // =========================================================
    // Sprite取得
    // =========================================================

    public Sprite GetStageSprite(int index)
    {
        if (index < 0 || index >= stageSprites.Count)
            return null;

        return stageSprites[index];
    }

    // =========================================================
    // 既存機能
    // =========================================================

    public int GetTotalStageCount() => stageOrder.Count;

    public string GetStageNameAt(int index)
    {
        return (index >= 0 && index < stageOrder.Count)
            ? stageOrder[index]
            : "";
    }

    public void StartFirstStage()
    {
        currentStageIndex = 0;
        LoadCurrentStage();
    }

    public void RetryFromBeginning()
    {
        currentStageIndex = 0;
        LoadCurrentStage();
    }

    public void RetryCurrentStage() => LoadCurrentStage();

    public void SelectStage(int index)
    {
        if (!IsStageUnlocked(index)) return; // ★安全対策追加

        currentStageIndex = index;
        LoadCurrentStage();
    }

    public void ClearStage()
    {
        int nextIndex = currentStageIndex + 1;

        if (nextIndex > ReachedStageIndex)
            ReachedStageIndex = nextIndex;

        currentStageIndex = nextIndex;

        if (currentStageIndex >= stageOrder.Count)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.GameClear);
            SceneController.Instance.LoadSceneAsync("GameClear");
            return;
        }

        LoadCurrentStage();
    }

    private void LoadCurrentStage()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stageOrder.Count) return;

        string sceneName = stageOrder[currentStageIndex];

        GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        SceneController.Instance.LoadSceneAsync(sceneName);
    }
}
