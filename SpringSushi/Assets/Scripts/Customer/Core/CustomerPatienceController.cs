using UnityEngine;

/// <summary>
/// Patienceの減少とAngry判定を担当する。
/// 気分による我慢時間の補正はCustomerMoodSO側(Strategy)に委譲する。
/// </summary>
public class CustomerPatienceController : MonoBehaviour
{
    private float currentPatience;
    private float maxPatience;
    private float basePatienceTime;
    private float patienceDecayRate;
    private CustomerMoodSO mood;
    private CustomerAI owner;

    public float PatienceRate => maxPatience > 0 ? currentPatience / maxPatience : 0f;

    public void Initialize(CustomerData data, CustomerAI ai, CustomerMoodSO moodSO = null)
    {
        basePatienceTime = data.basePatienceTime;
        patienceDecayRate = data.patienceDecayRate;
        owner = ai;
        mood = moodSO;

        maxPatience = mood != null ? mood.ModifyPatience(basePatienceTime) : basePatienceTime;
        currentPatience = maxPatience;
    }

    /// <summary>
    /// バッチ開始時にPatienceをリセットする。
    /// </summary>
    public void ResetPatience(int batchCount)
    {
        float decayed = basePatienceTime * Mathf.Pow(patienceDecayRate, batchCount - 1);
        maxPatience = mood != null ? mood.ModifyPatience(decayed) : decayed;
        currentPatience = maxPatience;
    }

    /// <summary>
    /// CustomerOrderFlowService.Tick() から毎フレーム呼ばれる。
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