using UnityEngine;

public class SushiMovement : MonoBehaviour
{
    [Header("Data")]
    public SushiData data;

    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public LaneSegment currentSegment;
    [Range(0, 1)] public float progress = 0f;

    private bool lastReversedState;

    public void Initialize(SushiData newData, LaneSegment startSegment)
    {
        data = newData;
        currentSegment = startSegment;
        progress = 0f;

        if (currentSegment != null) lastReversedState = currentSegment.isReversed;

        if (data?.sushiModel != null)
        {
            GameObject model = Instantiate(data.sushiModel, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (currentSegment == null) return;

        // 1. 反転検知
        if (currentSegment.isReversed != lastReversedState)
        {
            progress = 1.0f - progress;
            lastReversedState = currentSegment.isReversed;
            // 【追加ログ】どのセグメントが反転したか表示
            Debug.Log($"<color=orange>[Sushi Movement]</color> {currentSegment.name} の反転を検知！ 逆走を開始します。");
        }

        // --- (2.移動, 3.座標更新 はそのまま) ---
        float distance = Vector3.Distance(currentSegment.nodeA.Position, currentSegment.nodeB.Position);
        if (distance > 0.001f) progress += (moveSpeed / distance) * Time.deltaTime;
        UpdatePosition();

        // 4. 遷移
        if (progress >= 1.0f)
        {
            currentSegment = currentSegment.GetNextSegment();
            progress = 0f;
            if (currentSegment == null) SushiDestroy();
            else
            {
                lastReversedState = currentSegment.isReversed;
                // 【追加ログ】
                Debug.Log($"<color=white>[Sushi]</color> {currentSegment.name} に進入しました。");
            }
        }
    }

    private void UpdatePosition()
    {
        // GetEntry/ExitNodeがisReversedを見てA/Bを出し分けているので、これだけで逆走する
        Vector3 start = currentSegment.GetEntryNode().Position;
        Vector3 end = currentSegment.GetExitNode().Position;

        transform.position = Vector3.Lerp(start, end, progress);

        Vector3 dir = end - start;
        if (dir != Vector3.zero) transform.forward = dir;
    }

    public void SushiDestroy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sushi"))
        {
            Debug.Log($"<color=red>衝突消滅!</color> {data?.sushiName} が衝突しました。");
            SushiDestroy();
        }
    }
}