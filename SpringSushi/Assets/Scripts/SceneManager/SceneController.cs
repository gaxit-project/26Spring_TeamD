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
    [Tooltip("シーン読み込み完了後、ボタン操作を受け付けるまでの秒数")]
    [SerializeField] private float inputUnlockDelay = 1.0f;

    // シーン遷移中フラグ（全クラスから参照可能）
    public bool IsTransitioning { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 非同期でシーンを読み込む（フェード付き）。
    /// 遷移中の多重呼び出しは無視されます。
    /// </summary>
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

        // フェードアウト前に入力を封鎖
        SetInputActive(false);

        yield return Fade(1);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        yield return Fade(0);

        IsTransitioning = false;

        // フェードイン完了後、inputUnlockDelay 秒待ってから入力を解放
        yield return new WaitForSecondsRealtime(inputUnlockDelay);
        SetInputActive(true);

        Debug.Log($"[SceneController] 入力ロック解除（{inputUnlockDelay}秒経過）");
    }

    /// <summary>
    /// 新シーンの EventSystem を有効/無効にする。
    /// DontDestroyOnLoad の EventSystem がある場合はそちらも対象にする。
    /// </summary>
    private void SetInputActive(bool active)
    {
        // 現在シーン内の EventSystem をすべて検索（DontDestroyOnLoad含む）
        var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var es in systems)
        {
            es.enabled = active;
        }

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
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }
}