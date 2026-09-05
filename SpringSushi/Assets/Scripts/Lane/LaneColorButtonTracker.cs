using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 各色のレーン群の重心(ワールド座標)を把握し、LaneSelectorUIのスティック方向判定に提供する。
/// ボタン画像自体の位置はEditor上で配置したまま固定し、このスクリプトでは動かさない。
/// 行うのは、ステージに存在しない色のボタンの非表示化と、選択中の色のハイライト(点滅)のみ。
/// </summary>
public class LaneColorButtonTracker : MonoBehaviour
{
    [System.Serializable]
    public class ColorButtonEntry
    {
        public LaneColor color;
        public Image buttonImage; // 表示/非表示・色変更の対象(位置は固定のまま)
    }

    [Header("参照")]
    [SerializeField] private LaneSelectorUI laneSelectorUI;
    [SerializeField] private Camera mainCamera;

    [Header("色ごとのボタン設定")]
    [SerializeField] private List<ColorButtonEntry> buttons = new();

    [Header("見た目設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("選択中ボタンの点滅速度")]
    [SerializeField] private float blinkSpeed = 3f;

    // 色ごとの重心(ワールド座標)。ステージに存在しない色はエントリなし。
    private readonly Dictionary<LaneColor, Vector3> colorCentroids = new();

    /// <summary>
    /// 指定した色の、現在のスクリーン座標を取得する。
    /// LaneSelectorUIが、スティックの向きとの角度比較に使う。
    /// このステージに存在しない色の場合はfalseを返す。
    /// </summary>
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

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        ComputeCentroids();
    }

    /// <summary>
    /// シーン内の全LaneNode/LaneSegmentを走査し、色ごとにワールド座標の重心を求める。
    /// </summary>
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

            // このステージに存在しない色のボタンは非表示にする(位置は変更しない)
            entry.buttonImage.gameObject.SetActive(exists);
            if (!exists) continue;

            bool isSelected = selected.HasValue && selected.Value == entry.color;
            entry.buttonImage.color = isSelected
                ? Color.Lerp(normalColor, selectedColor, blink)
                : normalColor;
        }
    }
}