using System;
using UnityEngine;

/// <summary>
/// LaneColorごとのMaterialを管理するScriptableObject。
/// プロジェクトに1つ作成してLaneTileColorApplier.csにアサインする。
/// </summary>
[CreateAssetMenu(fileName = "LaneColorPalette", menuName = "Custom/LaneColorPalette")]
public class LaneColorPalette : ScriptableObject
{
    [Serializable]
    public class ColorEntry
    {
        public LaneColor laneColor;
        public Material material;
    }

    public ColorEntry[] entries;

    public Material GetMaterial(LaneColor color)
    {
        if (entries == null) return null;
        foreach (var entry in entries)
        {
            if (entry.laneColor == color)
                return entry.material;
        }
        return null;
    }
}