using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Column Clear", fileName = "ColumnClearAbility")]
public class ColumnClearAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearColumn(x);
       
    }
}