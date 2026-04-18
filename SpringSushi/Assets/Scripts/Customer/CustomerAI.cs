using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomerAI : MonoBehaviour
{
    [Header("UI設定")]
    public Image orderDisplayImage;
    public Slider patienceSlider;
    public GameObject canvasObj;

    private CustomerData data;      // ScriptableObjectを参照
    private SushiData wantSushi;
    private bool isWaiting = false;
    private float currentPatience;

    public void Initialize(CustomerData newData)
    {
        data = newData;

        // 見た目を生成（自分自身の子にする）
        if (data.customerPrefab != null)
        {
            Instantiate(data.customerPrefab, transform);
        }

        Debug.Log($"<color=lime>[Entry]</color> {data.customerType} が来店しました！");
    }

    private void Start()
    {
        canvasObj.SetActive(false);
        // 入店後、少し間を置いて注文を開始
        Invoke("CustomerOrder", 2f);
    }

    public void CustomerOrder()
    {
        SushiSpawner spawner = FindFirstObjectByType<SushiSpawner>();
        if (spawner != null && spawner.sushiDataList.Count > 0 && data != null)
        {
            wantSushi = spawner.sushiDataList[Random.Range(0, spawner.sushiDataList.Count)];

            // UI更新
            orderDisplayImage.sprite = wantSushi.sushiIcon;
            patienceSlider.value = 1f;
            currentPatience = data.patienceTime; // Dataから取得

            canvasObj.SetActive(true);
            isWaiting = true;
            Debug.Log($"<color=orange>[Order]</color> {data.customerType}が {wantSushi.sushiName} を注文。");
        }
    }

    private void Update()
    {
        if (isWaiting && data != null)
        {
            currentPatience -= Time.deltaTime;
            // スライダーの割合計算に data.patienceTime を直接使う
            patienceSlider.value = currentPatience / data.patienceTime;

            if (currentPatience <= 0)
            {
                Debug.Log($"<color=red>[Angry]</color> {data.customerType}が怒って帰りました。");
                CustomerExit();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isWaiting) return;

        if (other.CompareTag("Sushi"))
        {
            SushiMovement sushi = other.GetComponent<SushiMovement>();
            if (sushi != null && sushi.data == wantSushi)
            {
                Take(sushi);
            }
        }
    }

    private void Take(SushiMovement sushi)
    {
        isWaiting = false;
        Debug.Log($"<color=green>[Take]</color> {sushi.data.sushiName} をゲット！");
        sushi.SushiDestroy();
        StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        canvasObj.SetActive(false);
        Debug.Log($"<color=cyan>[Eat]</color> {data.customerType}が食事中...");

        // Dataから食べる時間を参照
        yield return new WaitForSeconds(data.eatTime);

        CustomerExit();
    }

    public void CustomerExit()
    {
        Destroy(gameObject);
    }
}