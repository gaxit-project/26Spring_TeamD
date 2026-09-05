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

    /// <summary>
    /// 現在選択中のレーンの色。選択可能な色が1つもない場合はnull。
    /// LaneGlowControllerが、発光させるべき色を判定するために参照する。
    /// </summary>
    public LaneColor? CurrentSelectedColor =>
        IsIndexAvailable(selectedIndex) ? colorOrder[selectedIndex] : (LaneColor?)null;

    private void Awake()
    {
        if (laneNetwork == null)
        {
            laneNetwork = FindObjectOfType<LaneNetwork>();
        }

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
        LaneInputManager.DirectInputDisabled = true;
        LaneSelectDecideInput.InputDisabled = false;
        availabilityBuilt = false;
    }

    private void OnDisable()
    {
        LaneInputManager.DirectInputDisabled = false;
        LaneSelectDecideInput.InputDisabled = true;
    }

    private void Update()
    {
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying)
        {
            return;
        }

        if (!availabilityBuilt)
        {
            BuildAvailability();
        }

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
                    UpdateHighlight();
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
            if (IsIndexAvailable(selectedIndex))
            {
                LaneColor color = colorOrder[selectedIndex];
                laneNetwork.ToggleLane(color);
            }
        }
    }

    private void UpdateHighlight()
    {
        if (buttonBackgrounds == null) return;

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