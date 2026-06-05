using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ワイプの吹き出しに表示される気分。
/// 注文傾向と感情効果を定義する。
/// </summary>
[CreateAssetMenu(fileName = "NewCustomerMood", menuName = "Custom/CustomerMood")]
public class CustomerMoodSO : ScriptableObject
{
    [Header("表示")]
    [Tooltip("吹き出しに表示するイラスト")]
    public Sprite moodIcon;

    [Header("気分の種類")]
    public MoodType moodType;

    [Header("特定寿司注文（MoodType=SpecificSushi のとき使用）")]
    [Tooltip("必ず注文する寿司リスト")]
    public List<SushiData> fixedOrders = new();

    [Header("腹ペコ設定（MoodType=Hungry のとき使用）")]
    [Tooltip("通常の注文数にこの倍率をかける（例：2 = 2倍注文）")]
    public float orderCountMultiplier = 2f;

    [Header("怒り設定（MoodType=Angry のとき使用）")]
    [Tooltip("PatienceTimeにかける倍率（例：0.5 = 半分の時間で怒る）")]
    [Range(0.1f, 1f)]
    public float patienceMultiplier = 0.5f;

    public enum MoodType
    {
        Random,       // ? ランダム注文（通常通り）
        SpecificSushi, // ?? 特定の寿司を必ず注文
        Hungry,       // ?? 腹ペコ・注文数が多い
        Irritated,    // ?? 怒り気味・Patienceが短い
    }
}