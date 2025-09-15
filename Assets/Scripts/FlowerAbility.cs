using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Flower Ability", menuName = "Flower Ability", order = 51)]
public class FlowerAbility : ScriptableObject
{
    public enum AbilityType { ClearRow, ClearColumn, ClearType, Bomb }
    public static readonly Dictionary<AbilityType, BoardAbility> AbilityMap = new Dictionary<AbilityType, BoardAbility>()
    {
        { AbilityType.ClearRow, new RowClearAbility() },
        { AbilityType.ClearColumn, new ColumnClearAbility() },
        { AbilityType.ClearType, new ClearTypeAbility() },
        { AbilityType.Bomb, new Bomb3x3Ability() }
    };
    
    public string title;
    public Cost price;
    public AbilityType ability;
    public string description;
}
