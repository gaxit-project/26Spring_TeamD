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
    [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("スティック選択設定")]
    [SerializeField] private float switchInterval = 0.3f;
    [SerializeField] private float stickDeadzone = 0.5f;

    private int selectedIndex = 0;
    private float switchTimer = 0f;
    private bool availabilityBuilt = false;
    private bool[] isAvailable;

    private void Awake()
    {
        // ★ buttonBackgrounds が未設定（手動アサイン忘れ、またはPrefab化対策）の場合、
        //    自身のビルダーや子要素からImageを自動で集める
        if (buttonBackgrounds == null || buttonBackgrounds.Length != colorOrder.Length)
        {
            // 例：このPrefabの子オブジェクトから順に Image を取得する
            // （子オブジェクトの並び順が Red, Blue, Green, Yellow である前提）
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
        // このUIが有効な間は、ダイレクト方式(X/Y/A/B)のレーン反転入力を無効化する
        LaneInputManager.DirectInputDisabled = true;
        availabilityBuilt = false; // 有効化されるたびに再構築を試みる
    }

    private void OnDisable()
    {
        LaneInputManager.DirectInputDisabled = false;
    }

    /// <summary>
    /// このステージに実際に存在する色だけを選択可能とし、
    /// 選択中インデックスも最初の選択可能な色に合わせる。
    /// LaneNetwork.Awake()がまだ実行されていない場合は失敗するので、
    /// Updateから毎フレーム再試行する。
    /// </summary>
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

    private void Update()
    {
        if (!availabilityBuilt)
        {
            BuildAvailability();
            if (availabilityBuilt) UpdateHighlight();
        }

        Debug.Log($"[LaneSelectorUI] Update: availabilityBuilt={availabilityBuilt}"); // ← 一時追加

        HandleStickSelection();
        HandleDecideButton();
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
                if (nextIndex >= 0) selectedIndex = nextIndex;

                switchTimer = switchInterval;
                UpdateHighlight();
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

        // IsPressedの代わりに、新しく作った WasPressedThisFrame を使う
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

        for (int i = 0; i < buttonBackgrounds.Length; i++)
        {
            if (buttonBackgrounds[i] == null) continue;

            if (!IsIndexAvailable(i))
            {
                buttonBackgrounds[i].color = disabledColor;
            }
            else
            {
                buttonBackgrounds[i].color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }
    }
}