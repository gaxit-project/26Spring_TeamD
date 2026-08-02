using UnityEngine;

/// <summary>
/// 来店スケジュールの1単位（波）。
/// countを2以上にすると、その波の客は全員まとめて同時に入店する。
/// 例：[count=1,delay=3][count=1,delay=3][count=2,delay=3] なら
///     1人 → 3秒後に1人 → 3秒後に2人同時、という順で来店する。
/// </summary>
[System.Serializable]
public class CustomerArrivalWave
{
    [Tooltip("この波で同時に来店させる人数（2以上でグループ入店になる）")]
    [Min(1)] public int count = 1;

    [Tooltip("直前の波の来店から、この波が来店するまでの待機時間（秒）")]
    [Min(0)] public float delay = 3f;
}