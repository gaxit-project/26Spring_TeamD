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
    private CustomerAI owner;

    public float PatienceRate => maxPatience > 0 ? currentPatience / maxPatience : 0f;

    public void Initialize(CustomerData data, CustomerAI ai)
    {
        basePatienceTime = data.basePatienceTime;
        patienceDecayRate = data.patienceDecayRate;
        owner = ai;
        maxPatience = basePatienceTime;
        currentPatience = maxPatience;
    }

    /// <summary>
    /// バッチ開始時にPatienceをリセットする。
    /// </summary>
    public void ResetPatience(int batchCount)
    {
        maxPatience = basePatienceTime * Mathf.Pow(patienceDecayRate, batchCount - 1);
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