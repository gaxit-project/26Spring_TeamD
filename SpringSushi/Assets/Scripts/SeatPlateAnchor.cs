using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// この座席(テーブル)における「食べた寿司を置く場所」のリストを示す。
/// テーブルの向きは座席ごとに異なるため、客側ではなく座席側がこの情報を持つ。
/// 各要素は座席プレハブの子として、テーブル上の実際の置き場所に
/// あらかじめ手動で配置・調整しておく。配達された寿司は、リストの順番通りに
/// 1つずつこの位置へ置かれる。
/// SeatBuilderがどの向きで座席を生成しても、子オブジェクトとして自動的に追従する。
/// </summary>
public class SeatPlateAnchor : MonoBehaviour
{
    [Tooltip("寿司を置く場所のリスト。テーブル上の実際の置き場所に、" +
             "1つずつ手動で配置・調整する。同時に出る最大皿数ぶん用意しておく")]
    public List<Transform> plateSlots = new();
}