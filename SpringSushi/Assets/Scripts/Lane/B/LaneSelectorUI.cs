using UnityEngine;
using UnityEngine.UI;

public class LaneSelectorUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LaneNetwork laneNetwork;

    [Header("ボタン表示順（左から右へ）")]
    [SerializeField] private LaneColor[] colorOrder = { LaneColor.Red, LaneColor.Blue, LaneColor.Green, LaneColor.Yellow };

    [Header("UI参照（自動取得する場合は空でも可）")]
    [Tooltip("手動でアサインしない場合、子要素から自動で取得します")]
    [SerializeField] private Image[] buttonBackgrounds;

    [Header("ハイライト設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.red;

    [Header("フェード点滅設定")]
    [Tooltip("点滅の速さ（1往復にかかる時間や周期の調整）")]
    [SerializeField] private float fadeSpeed = 3.0f;

    [Header("スティック選択設定")]
    [Tooltip("スティックを倒し続けたとき選択が切り替わる間隔（秒）")]
    [SerializeField] private float switchInterval = 0.3f;
    [SerializeField] private float stickDeadzone = 0.5f;

    private int selectedIndex = 0;
    private float switchTimer = 0f;
    private bool availabilityBuilt = false;
    private bool[] isAvailable;

    private void Awake()
    {
        // LaneNetworkが未設定なら自動でシーン内から探す（Prefab対策）
        if (laneNetwork == null)
        {
            laneNetwork = FindObjectOfType<LaneNetwork>();
        }

        // buttonBackgrounds が未設定の場合、子要素からImageを自動で集める
        if (buttonBackgrounds == null || buttonBackgrounds.Length != colorOrder.Length)
        {
            buttonBackgrounds = new Image[colorOrder.Length];
            for (int i = 0; i < colorOrder.Length; i++)
            {
                if (transform.childCount > i)
                {
                    buttonBackgrounds[i] = transform.GetChild(i).GetComponent<Image>();
                }
            }
        }
    }

    private void OnEnable()
    {
        // ダイレクト方式を無効化
        LaneInputManager.DirectInputDisabled = true;
        // 選択方式のAボタン決定も許可状態にする
        LaneSelectDecideInput.InputDisabled = false;
        availabilityBuilt = false;
    }

    private void OnDisable()
    {
        // ダイレクト方式を元に戻す
        LaneInputManager.DirectInputDisabled = false;
        // 選択方式のAボタン決定も無効化する
        LaneSelectDecideInput.InputDisabled = true;
    }

    private void Update()
    {
        // ★ プレイ中（Playing）以外（ポーズ中やタイトル等）なら何もしない
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying)
        {
            return;
        }

        // まだ利用可能フラグが立っていなければ構築を試みる
        if (!availabilityBuilt)
        {
            BuildAvailability();
        }

        // availabilityBuilt が立っていれば、フェード点滅のため毎フレームハイライトを更新する
        if (availabilityBuilt)
        {
            UpdateHighlight();
        }

        HandleStickSelection();
        HandleDecideButton();
    }

    private void BuildAvailability()
    {
        if (laneNetwork == null) return;

        isAvailable = new bool[colorOrder.Length];

        for (int i = 0; i < colorOrder.Length; i++)
            isAvailable[i] = laneNetwork.HasLane(colorOrder[i]);

        if (!IsIndexAvailable(selectedIndex))
        {
            int firstAvailable = FindFirstAvailableIndex();
            if (firstAvailable >= 0) selectedIndex = firstAvailable;
        }

        availabilityBuilt = FindFirstAvailableIndex() >= 0;
    }

    private bool IsIndexAvailable(int index)
    {
        return isAvailable != null && index >= 0 && index < isAvailable.Length && isAvailable[index];
    }

    private int FindFirstAvailableIndex()
    {
        if (isAvailable == null) return -1;
        for (int i = 0; i < isAvailable.Length; i++)
            if (isAvailable[i]) return i;
        return -1;
    }

    private void HandleStickSelection()
    {
        if (!availabilityBuilt) return;

        float horizontal = SpawnerInputManager.LeftStickValue.x;

        if (Mathf.Abs(horizontal) > stickDeadzone)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
            {
                int dir = horizontal > 0 ? 1 : -1;
                int nextIndex = FindNextAvailableIndex(selectedIndex, dir);
                if (nextIndex >= 0)
                {
                    selectedIndex = nextIndex;
                    UpdateHighlight(); // ← 選択が変わった瞬間にもハイライトを更新
                }

                switchTimer = switchInterval;
            }
        }
        else
        {
            switchTimer = 0f;
        }
    }

    private int FindNextAvailableIndex(int start, int dir)
    {
        if (isAvailable == null || isAvailable.Length == 0) return start;

        int count = colorOrder.Length;
        for (int step = 1; step <= count; step++)
        {
            int candidate = (start + dir * step + count) % count;
            if (isAvailable[candidate]) return candidate;
        }
        return -1;
    }

    private void HandleDecideButton()
    {
        if (!availabilityBuilt) return;

        if (LaneSelectDecideInput.WasPressedThisFrame)
        {
            Debug.Log($"[LaneSelectorUI] selectedIndex={selectedIndex}, color={colorOrder[selectedIndex]}, isAvailable={IsIndexAvailable(selectedIndex)}");

            if (IsIndexAvailable(selectedIndex))
            {
                LaneColor color = colorOrder[selectedIndex];
                Debug.Log($"[LaneSelectorUI] laneNetwork参照={laneNetwork}, InstanceID={laneNetwork.GetInstanceID()}");
                laneNetwork.ToggleLane(color);
                Debug.Log($"[LaneSelectorUI] ToggleLane呼び出し完了");
            }
        }
    }

    private void UpdateHighlight()
    {
        if (buttonBackgrounds == null) return;

        // Mathf.PingPong を使って 0.0 ～ 1.0 の間を滑らかに往復させる
        float t = Mathf.PingPong(Time.time * fadeSpeed, 1f);

        for (int i = 0; i < buttonBackgrounds.Length; i++)
        {
            if (buttonBackgrounds[i] == null) continue;

            if (!IsIndexAvailable(i))
            {
                buttonBackgrounds[i].gameObject.SetActive(false);
            }
            else
            {
                buttonBackgrounds[i].gameObject.SetActive(true);

                if (i == selectedIndex)
                {
                    // normalColor と selectedColor の間を滑らかに補間（フェード）させる
                    buttonBackgrounds[i].color = Color.Lerp(normalColor, selectedColor, t);
                }
                else
                {
                    buttonBackgrounds[i].color = normalColor;
                }
            }
        }
    }
}