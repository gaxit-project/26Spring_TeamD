using UnityEngine;

/// <summary>
/// Patienceの減少とAngry判定を担当する。
/// </summary>
public class CustomerPatienceController : MonoBehaviour
{
    private float currentPatience;
    private float maxPatience;
    private float basePatienceTime;
    private float patienceDecayRate;
    private float currentMultiplier = 1f; // ★ 追加: 気分による倍率を保持する
    private CustomerAI owner;

    public float PatienceRate => maxPatience > 0 ? currentPatience / maxPatience : 0f;

    public void Initialize(CustomerData data, CustomerAI ai, CustomerMoodSO mood = null)
    {
        basePatienceTime = data.basePatienceTime;
        patienceDecayRate = data.patienceDecayRate;
        owner = ai;

        // ★ Irritated なら Patience を短くする倍率を決定
        currentMultiplier = (mood != null && mood.moodType == CustomerMoodSO.MoodType.Irritated)
            ? mood.patienceMultiplier
            : 1f;

        maxPatience = basePatienceTime * currentMultiplier;
        currentPatience = maxPatience;
    }

    /// <summary>
    /// バッチ開始時にPatienceをリセットする。
    /// </summary>
    public void ResetPatience(int batchCount)
    {
        // ★ 修正: 2回目以降の注文リセット時にも、currentMultiplier（怒り倍率）をしっかり掛ける
        maxPatience = basePatienceTime * Mathf.Pow(patienceDecayRate, batchCount - 1) * currentMultiplier;
        currentPatience = maxPatience;
    }

    /// <summary>
    /// CustomerAI.Update() から毎フレーム呼ばれる。
    /// </summary>
    public void Tick(float deltaTime)
    {
        currentPatience -= deltaTime;
        owner.NotifyPatienceChanged();

        if (currentPatience <= 0f)
        {
            currentPatience = 0f;
            owner.NotifyAngry();
        }
    }
}