using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("フェード設定")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("入力ロック設定")]
    [Tooltip("通常シーン（タイトル・リザルト等）でのロック解除待機時間（秒）")]
    [SerializeField] private float inputUnlockDelay = 1.0f;

    [Tooltip("この名前を含むシーンではinputUnlockDelayをゼロにする\n（StageGoalUIが入力タイミングを制御するため）")]
    [SerializeField] private string[] stageSceneNames = { "Stage" };

    public bool IsTransitioning { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSceneAsync(string sceneName)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning($"[SceneController] LoadSceneAsync({sceneName}) は遷移中のため無視されました。");
            return;
        }
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        IsTransitioning = true;
        SetInputActive(false);

        yield return Fade(1);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        yield return Fade(0);

        IsTransitioning = false;

        // ★ ステージシーンは StageGoalUI が入力タイミングを管理するため
        //    SceneController 側のロック解除待機はスキップする
        if (IsStageScene(sceneName))
        {
            SetInputActive(true);
            Debug.Log($"[SceneController] ステージシーンのため inputUnlockDelay をスキップ");
        }
        else
        {
            yield return new WaitForSecondsRealtime(inputUnlockDelay);
            SetInputActive(true);
            Debug.Log($"[SceneController] 入力ロック解除（{inputUnlockDelay}秒経過）");
        }
    }

    private bool IsStageScene(string sceneName)
    {
        foreach (var keyword in stageSceneNames)
        {
            if (sceneName.Contains(keyword)) return true;
        }
        return false;
    }

    private void SetInputActive(bool active)
    {
        var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var es in systems)
            es.enabled = active;
        Debug.Log($"[SceneController] EventSystem {(active ? "有効" : "無効")} (count:{systems.Length})");
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null) yield break;
        float startAlpha = fadeImage.color.a;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }
}