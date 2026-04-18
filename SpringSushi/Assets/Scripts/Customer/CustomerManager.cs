using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject customerPrefab;
    public List<CustomerData> customerVariationList;
    public List<Transform> entryPoints = new List<Transform>();
    public float spawnInterval = 5f;

    private void Start()
    {
        if (customerVariationList == null || customerVariationList.Count == 0) return;
        StartCoroutine(CustomerEntryRoutine());
    }

    private IEnumerator CustomerEntryRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            CustomerEntry();
        }
    }

    public void CustomerEntry()
    {
        if (customerPrefab == null || entryPoints.Count == 0) return;

        // 1. 空いている席を探す（ここが重要！）
        Transform targetSeat = GetEmptySeat();

        // 2. 空いている席がある時だけ、クローンを生成する
        if (targetSeat != null)
        {
            CustomerData randomData = customerVariationList[Random.Range(0, customerVariationList.Count)];
            GameObject obj = Instantiate(customerPrefab, targetSeat.position, targetSeat.rotation);

            CustomerAI ai = obj.GetComponent<CustomerAI>();
            if (ai != null)
            {
                ai.Initialize(randomData);
            }
            Debug.Log($"<color=lime>[Manager]</color> {targetSeat.name} に客を生成しました。");
        }
        else
        {
            // 空席がない場合は何もしない（クローンも作らない）
            Debug.Log("<color=yellow>[Manager]</color> 満席のため、生成をスキップしました。");
        }
    }

    private Transform GetEmptySeat()
    {
        // 全ての登録された席（entryPoints）を一つずつチェック
        foreach (var seat in entryPoints)
        {
            // 席の座標を中心に半径 0.5m の球体内にコライダーがあるか調べる
            Collider[] colliders = Physics.OverlapSphere(seat.position, 0.5f);
            bool isOccupied = false;

            foreach (var col in colliders)
            {
                // 「Customer」タグが付いているオブジェクトが一人でもいたら、その席は「埋まっている」
                if (col.CompareTag("Customer"))
                {
                    isOccupied = true;
                    break;
                }
            }

            // この席に誰もいなければ、この席を返す
            if (!isOccupied) return seat;
        }

        // 全ての席を回った結果、空きがなければ null（なし）を返す
        return null;
    }
}