using UnityEngine;

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

    public void ResetPatience(int batchCount)
    {
        float decayed = basePatienceTime * Mathf.Pow(patienceDecayRate, batchCount - 1);
        maxPatience = mood != null ? mood.ModifyPatience(decayed) : decayed;
        currentPatience = maxPatience;
    }

    public void Tick(float deltaTime)
    {
        // ★ フィーバーフェーズ中はお客さんの我慢度が減らない
        if (FeverManager.Instance != null && FeverManager.Instance.IsFever)
        {
            return;
        }

        currentPatience -= deltaTime;
        owner.NotifyPatienceChanged();

        if (currentPatience <= 0f)
        {
            currentPatience = 0f;
            owner.NotifyAngry();
        }
    }
}