using System;
using UnityEngine;

public class ColumnClearAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearColumn(x);
       
    }
}