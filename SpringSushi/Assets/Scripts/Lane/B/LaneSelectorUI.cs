using UnityEngine;

public class LaneSelectorUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LaneNetwork laneNetwork;
    [SerializeField] private LaneColorButtonTracker colorButtonTracker;

    [Header("スティック選択のサイクル順（フォールバック用）")]
    [Tooltip("画面座標が取得できない場合に使う、順送り選択の順番")]
    [SerializeField] private LaneColor[] colorOrder = { LaneColor.Red, LaneColor.Blue, LaneColor.Green, LaneColor.Yellow };

    [Header("スティック選択設定")]
    [Tooltip("この入力量を下回る場合は無入力とみなす")]
    [SerializeField] private float stickDeadzone = 0.5f;
    [Tooltip("同じ方向へ切り替わり続けないための、切り替え後の再入力までの間隔（秒）")]
    [SerializeField] private float switchInterval = 0.3f;

    private int selectedIndex = 0;
    private float switchTimer = 0f;
    private bool availabilityBuilt = false;
    private bool[] isAvailable;

    /// <summary>
    /// 現在選択中のレーンの色。選択可能な色が1つもない場合はnull。
    /// LaneGlowController・LaneColorButtonTrackerが、表示に反映するために参照する。
    /// </summary>
    public LaneColor? CurrentSelectedColor =>
        IsIndexAvailable(selectedIndex) ? colorOrder[selectedIndex] : (LaneColor?)null;

    private void Awake()
    {
        if (laneNetwork == null)
        {
            laneNetwork = FindObjectOfType<LaneNetwork>();
        }
        if (colorButtonTracker == null)
        {
            colorButtonTracker = FindObjectOfType<LaneColorButtonTracker>();
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

    /// <summary>
    /// 左スティックの入力ベクトル(X,Y両方)を使い、
    /// 現在の選択位置から見て、スティックの向きに最も近い方向にある色へ切り替える。
    /// 画面上の位置情報が取得できない場合は、colorOrderに沿った左右の順送りにフォールバックする。
    /// </summary>
    private void HandleStickSelection()
    {
        if (!availabilityBuilt) return;

        Vector2 stick = SpawnerInputManager.LeftStickValue;
        if (stick.sqrMagnitude < stickDeadzone * stickDeadzone)
        {
            switchTimer = 0f;
            return;
        }

        switchTimer -= Time.deltaTime;
        if (switchTimer > 0f) return;

        int nextIndex = FindNearestByDirection(stick);
        if (nextIndex < 0)
        {
            // 画面座標が取得できない場合のフォールバック：X軸だけ見て順送り
            int dir = stick.x > 0 ? 1 : -1;
            nextIndex = FindNextAvailableIndexCyclic(selectedIndex, dir);
        }

        if (nextIndex >= 0 && nextIndex != selectedIndex)
        {
            selectedIndex = nextIndex;
            switchTimer = switchInterval;

            if (SoundPlayer.Instance != null)
                SoundPlayer.Instance.PlaySFX(SoundKeys.LaneCursorMove);
        }
    }

    /// <summary>
    /// 現在選択中の色の画面座標を基準に、stickDir方向に最も近い他の色を探す。
    /// Unityのスクリーン座標はY軸が上向き正、スティックのYも上向き正のため、そのまま使える。
    /// </summary>
    private int FindNearestByDirection(Vector2 stickDir)
    {
        if (colorButtonTracker == null) return -1;
        if (!colorButtonTracker.TryGetScreenPosition(colorOrder[selectedIndex], out Vector2 currentPos))
            return -1;

        stickDir = stickDir.normalized;

        int bestIndex = -1;
        float bestScore = -2f; // 内積の最小値(-1)より小さい初期値

        for (int i = 0; i < colorOrder.Length; i++)
        {
            if (i == selectedIndex) continue;
            if (!IsIndexAvailable(i)) continue;

            if (!colorButtonTracker.TryGetScreenPosition(colorOrder[i], out Vector2 candidatePos))
                continue;

            Vector2 toCandidate = candidatePos - currentPos;
            if (toCandidate.sqrMagnitude < 0.0001f) continue; // 同じ位置ならスキップ

            Vector2 dirToCandidate = toCandidate.normalized;

            // スティックの向きと、候補への方向の近さを内積で評価する(1に近いほど同じ向き)
            float score = Vector2.Dot(stickDir, dirToCandidate);

            // 真逆?直角に近い候補は選ばせない(意図しない方向へ飛ぶのを防ぐ)
            if (score < 0.3f) continue;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    /// <summary>フォールバック用：colorOrderに沿った単純な順送り。</summary>
    private int FindNextAvailableIndexCyclic(int start, int dir)
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

                if (colorButtonTracker != null)
                    colorButtonTracker.FlashPress(color);

                if (SoundPlayer.Instance != null)
                    SoundPlayer.Instance.PlaySFX(SoundKeys.LaneDecide);
            }
        }
    }
}