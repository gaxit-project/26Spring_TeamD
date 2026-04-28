using UnityEngine;

/// <summary>
/// LaneSegmentの見た目を管理する。
/// IsReversedの変化を受けてレーンアニメーションの方向を反転させる。
///
/// 【Prefab構成の想定】
/// LaneSegment (GameObject)
///   └ LaneVisual (GameObject) ← このスクリプトをアタッチ
///       └ レーンのMesh/Animator
///
/// アニメーションの反転はAnimatorのspeedを-1にする方式を使用。
/// Animatorがない場合はMaterialのtextureScaleでUVスクロール方向を反転する。
/// </summary>
[RequireComponent(typeof(Renderer))]
public class LaneSegmentView : MonoBehaviour
{
    [SerializeField] private LaneSegment segment;

    [Header("アニメーション設定")]
    [SerializeField] private Animator laneAnimator;
    [Tooltip("Animatorがない場合、MaterialのUVスクロールで方向を表現する")]
    [SerializeField] private bool useUVScroll = false;
    [SerializeField] private float uvScrollSpeed = 0.5f;
    [SerializeField] private string uvScrollTextureName = "_MainTex";

    private Renderer rend;
    private float currentScrollOffset = 0f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        if (segment == null)
            segment = GetComponentInParent<LaneSegment>();

        segment.OnReversedChanged += UpdateView;
        UpdateView(segment.IsReversed);
    }

    private void Update()
    {
        if (!useUVScroll || rend == null) return;

        float direction = segment.IsReversed ? -1f : 1f;
        currentScrollOffset += uvScrollSpeed * direction * Time.deltaTime;
        currentScrollOffset %= 1f;

        // MaterialPropertyBlockで他のインスタンスに影響を与えない
        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);
        block.SetVector(uvScrollTextureName, new Vector4(1, 1, currentScrollOffset, 0));
        rend.SetPropertyBlock(block);
    }

    private void UpdateView(bool isReversed)
    {
        // Animatorがある場合：speedを反転
        if (laneAnimator != null)
        {
            laneAnimator.speed = isReversed ? -1f : 1f;
            return;
        }

        // Animatorがない場合：色で仮表示（既存の挙動を維持）
        if (rend != null)
            rend.material.color = isReversed ? Color.black : Color.white;
    }

    private void OnDestroy()
    {
        if (segment != null)
            segment.OnReversedChanged -= UpdateView;
    }
}