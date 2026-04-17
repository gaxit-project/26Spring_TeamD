using UnityEngine;

[CreateAssetMenu(fileName = "NewSushiObject", menuName = "Custom/SushiObjectData")]
public class SushiData : ScriptableObject
{
    public string sushiName;
    public int price;
    public GameObject sushiModel;
}
