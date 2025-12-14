using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/LevelData", order = 0)]
public class LevelData : ScriptableObject
{
    public int LevelNumber;
    public int Rows;
    public int Columns;

    public List<BlockData> Blocks = new();
    public List<BoxData> Boxes = new();
}