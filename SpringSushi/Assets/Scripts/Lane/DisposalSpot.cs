using UnityEngine;

/// <summary>
/// LaneNodeにアタッチする、ゴミ箱の見た目を持つDisposalSpot用のマーカー。
/// SushiSpawnerと同様、LaneNodeへの追加コンポーネントとして機能する。
/// このノードに到達した寿司は、次のレーンへ送られる代わりに廃棄される。
/// 廃棄すると、寿司価格の1/10を減点し、専用の廃棄音を鳴らす。
/// ただし、フィーバー中は減点されない(SushiMovementの衝突処理と同じ扱い)。
/// </summary>
[RequireComponent(typeof(LaneNode))]
public class DisposalSpot : MonoBehaviour
{
    [Header("見た目")]
    [Tooltip("ゴミ箱の見た目のインスタンス。NodeArrowBuilderが自動生成する")]
    [SerializeField] private GameObject visualInstance;

    public void Dispose(SushiMovement sushi)
    {
        if (sushi == null || sushi.data == null) return;

        bool isFever = FeverManager.Instance != null && FeverManager.Instance.IsFever;

        if (!isFever)
        {
            int lossAmount = Mathf.RoundToInt(sushi.data.price / 10f);
            ScoreManager.Instance?.SubtractScore(lossAmount);
        }

        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.PlaySFX(SoundKeys.SushiDispose);

        sushi.SushiDestroy();
    }

#if UNITY_EDITOR
    public void EditorSetVisual(GameObject instance)
    {
        visualInstance = instance;
    }
#endif
}