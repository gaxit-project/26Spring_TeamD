using UnityEngine;

/// <summary>
/// ステージごとの売上目標を定義するScriptableObject。
/// Assets/StageGoals/ に置いて各ステージにアサインする。
/// </summary>
[CreateAssetMenu(fileName = "StageGoalData", menuName = "Game/StageGoalData")]
public class StageGoalData : ScriptableObject
{
    [Header("ステージ名（表示用）")]
    public string stageName = "ステージ1";

    [Header("売上目標（円）")]
    [Tooltip("★☆☆ ブロンズ: 基本操作ができれば到達")]
    public int goalBronze = 1500;

    [Tooltip("★★☆ シルバー: 効率よくレーンを回す必要がある")]
    public int goalSilver = 3000;

    [Tooltip("★★★ ゴールド: ノーミスの職人技が必要")]
    public int goalGold = 10000;

    /// <summary>スコアから星の数（0?3）を返す</summary>
    public int GetStarCount(int score)
    {
        if (score >= goalGold) return 3;
        if (score >= goalSilver) return 2;
        if (score >= goalBronze) return 1;
        return 0;
    }
}