using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SushiSpawner : MonoBehaviour
{
    public GameObject sushiBasePrefab;
    public List<SushiData> sushiDataList;
    public float spawnInterval = 3.0f;

    private LaneNode myNode;
    private SushiRegistry registry;

    public void SetMasterNode(LaneNode master)
    {
        myNode = master;
    }

    private void Start()
    {
        registry = FindFirstObjectByType<SushiRegistry>();

        if (myNode == null)
            myNode = GetComponent<LaneNode>();

        StartCoroutine(SafeStart());
    }

    private IEnumerator SafeStart()
    {
        yield return null;

        if (myNode != null)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSushi();
        }
    }

    private void SpawnSushi()
    {
        if (sushiBasePrefab == null || sushiDataList.Count == 0 || myNode == null)
            return;

        LaneSegment targetSegment = myNode.connectedSegments.Count > 0 ? myNode.connectedSegments[0] : null;
        if (targetSegment == null) return;

        GameObject obj = Instantiate(sushiBasePrefab, myNode.Position, Quaternion.identity);

        SushiMovement move = obj.GetComponent<SushiMovement>();
        SushiData data = sushiDataList[Random.Range(0, sushiDataList.Count)];

        move.Initialize(data, targetSegment, myNode, registry);
    }
}