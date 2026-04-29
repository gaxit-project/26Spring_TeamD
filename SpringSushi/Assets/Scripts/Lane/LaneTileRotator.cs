using UnityEngine;

/// <summary>
/// LaneSegmentのIsReversed変化を受けて、
/// TileをY軸180度スナップ回転させるコンポーネント。
///
/// 【アタッチ先】
/// LaneSegment
///   └─ Tile_00 ← ここにアタッチ
///        ├─ LaneBase
///        └─ Lane
///
/// 初期回転（IsReversed=false時）はAwake時のtransform.rotationを基準にする。
/// LaneTileBuilderが生成時に正しい向きで配置するため、手動設定不要。
/// </summary>
public class LaneTileRotator : MonoBehaviour
{
    [SerializeField] private LaneSegment segment;

    // Awake時のワールド回転を基準として保持する
    private Quaternion baseRotation;

    private void Awake()
    {
        // 配置時の回転を基準として記録
        baseRotation = transform.rotation;

        if (segment == null)
            segment = GetComponentInParent<LaneSegment>();

        if (segment == null)
        {
            Debug.LogWarning($"[LaneTileRotator] {gameObject.name}: 親にLaneSegmentが見つかりません。");
            return;
        }

        segment.OnReversedChanged += OnReversedChanged;

        // 初期状態を反映（すでにIsReversed=trueの場合に対応）
        ApplyRotation(segment.IsReversed);
    }

    private void OnReversedChanged(bool isReversed)
    {
        ApplyRotation(isReversed);
    }

    private void ApplyRotation(bool isReversed)
    {
        if (isReversed)
            transform.rotation = baseRotation * Quaternion.Euler(0f, 180f, 0f);
        else
            transform.rotation = baseRotation;
    }

    private void OnDestroy()
    {
        if (segment != null)
            segment.OnReversedChanged -= OnReversedChanged;
    }
}