using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 客の気分の基底クラス。気分ごとの振る舞いはサブクラスで実装する(Strategyパターン)。
/// 新しい気分を追加する際は、このクラスを継承した新しいSOを作るだけでよく、
/// OrderGenerator/CustomerPatienceController側の既存コードを触る必要はない。
/// </summary>
public abstract class CustomerMoodSO : ScriptableObject
{
    [Header("表示")]
    [Tooltip("吹き出しに表示するイラスト")]
    public Sprite moodIcon;

    /// <summary>
    /// ワイプの吹き出し・気分アイコンを表示すべきかどうか。
    /// 「特に特徴のない気分」はfalseを返すことで、旧MoodType.Randomと同じ見た目になる。
    /// </summary>
    public virtual bool ShowIndicator => true;

    /// <summary>基本の我慢時間(basePatienceTime)に対して、この気分による補正後の値を返す。</summary>
    public abstract float ModifyPatience(float basePatienceTime);

    /// <summary>この気分に応じた注文リストを生成する。availableSushiListはステージ側の候補一覧。</summary>
    public abstract List<SushiData> GenerateOrders(CustomerData data, List<SushiData> availableSushiList);
}