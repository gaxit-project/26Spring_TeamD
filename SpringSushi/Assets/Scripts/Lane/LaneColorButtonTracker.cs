using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneColorButtonTracker : MonoBehaviour
{
    [System.Serializable]
    public class ColorButtonEntry
    {
        public LaneColor color;
        public Image buttonImage;
        [System.NonSerialized] public float pressFlashTimer;
    }

    [Header("参照")]
    [SerializeField] private LaneSelectorUI laneSelectorUI;
    [SerializeField] private Camera mainCamera;

    [Header("色ごとのボタン設定")]
    [SerializeField] private List<ColorButtonEntry> buttons = new();

    [Header("見た目設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("決定ボタンを押した瞬間に一瞬表示する色")]
    [SerializeField] private Color pressColor = Color.red;
    [Tooltip("選択中ボタンの点滅速度")]
    [SerializeField] private float blinkSpeed = 3f;
    [Tooltip("press色を表示し続ける時間（秒）")]
    [SerializeField] private float pressFlashDuration = 0.15f;

    /// <summary>
    /// 決定ボタンが押されてPressColor演出が開始された時に発火するイベント
    /// (対象の色, 演出時間)
    /// </summary>
    public event Action<LaneColor, float> OnPressFlashed;

    private readonly Dictionary<LaneColor, Vector3> colorCentroids = new();

    public bool TryGetScreenPosition(LaneColor color, out Vector2 screenPos)
    {
        screenPos = Vector2.zero;
        if (!colorCentroids.TryGetValue(color, out Vector3 worldPos)) return false;
        if (mainCamera == null) return false;

        Vector3 sp = mainCamera.WorldToScreenPoint(worldPos);
        if (sp.z < 0f) return false;

        screenPos = new Vector2(sp.x, sp.y);
        return true;
    }

    public void FlashPress(LaneColor color)
    {
        foreach (var entry in buttons)
        {
            if (entry.color == color)
            {
                entry.pressFlashTimer = pressFlashDuration;
                break;
            }
        }

        // 赤Flame連動用にイベントを発火
        OnPressFlashed?.Invoke(color, pressFlashDuration);
    }

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        ComputeCentroids();
    }

    private void ComputeCentroids()
    {
        var sums = new Dictionary<LaneColor, Vector3>();
        var counts = new Dictionary<LaneColor, int>();

        foreach (var node in FindObjectsByType<LaneNode>(FindObjectsSortMode.None))
        {
            AccumulatePosition(sums, counts, node.laneColor, node.Position);
        }

        foreach (var seg in FindObjectsByType<LaneSegment>(FindObjectsSortMode.None))
        {
            if (seg.nodeA == null || seg.nodeB == null) continue;
            Vector3 mid = (seg.nodeA.Position + seg.nodeB.Position) * 0.5f;
            AccumulatePosition(sums, counts, seg.laneColor, mid);
        }

        colorCentroids.Clear();
        foreach (var kv in sums)
        {
            int count = counts[kv.Key];
            if (count > 0)
                colorCentroids[kv.Key] = kv.Value / count;
        }
    }

    private void AccumulatePosition(Dictionary<LaneColor, Vector3> sums, Dictionary<LaneColor, int> counts,
                                     LaneColor color, Vector3 pos)
    {
        if (color == LaneColor.NoColor) return;

        if (!sums.ContainsKey(color))
        {
            sums[color] = Vector3.zero;
            counts[color] = 0;
        }
        sums[color] += pos;
        counts[color]++;
    }

    private void Update()
    {
        LaneColor? selected = laneSelectorUI != null ? laneSelectorUI.CurrentSelectedColor : null;
        float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        foreach (var entry in buttons)
        {
            if (entry.buttonImage == null) continue;

            bool exists = colorCentroids.ContainsKey(entry.color);

            entry.buttonImage.gameObject.SetActive(exists);
            if (!exists) continue;

            if (entry.pressFlashTimer > 0f)
            {
                entry.pressFlashTimer -= Time.deltaTime;
                entry.buttonImage.color = pressColor;
                continue;
            }

            bool isSelected = selected.HasValue && selected.Value == entry.color;
            entry.buttonImage.color = isSelected
                ? Color.Lerp(normalColor, selectedColor, blink)
                : normalColor;
        }
    }
}