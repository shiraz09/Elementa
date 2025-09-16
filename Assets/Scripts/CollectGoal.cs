using UnityEngine;

[System.Serializable]
public class CollectGoal
{
    public Grid.PieceType type;   // EARTH/GRASS/WATER/SUN
    public int target = 0;        // כמה צריך לאסוף
    [HideInInspector] public int current = 0;
}