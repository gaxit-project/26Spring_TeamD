using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LaneSegmentView : MonoBehaviour
{
    [SerializeField] private LaneSegment segment;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        if (segment == null)
            segment = GetComponent<LaneSegment>();

        segment.OnReversedChanged += UpdateView;

        // 初期状態反映（重要）
        UpdateView(segment.IsReversed);
    }

    private void UpdateView(bool isReversed)
    {
        rend.material.color = isReversed ? Color.black : Color.white;
    }

    private void OnDestroy()
    {
        if (segment != null)
            segment.OnReversedChanged -= UpdateView;
    }
}