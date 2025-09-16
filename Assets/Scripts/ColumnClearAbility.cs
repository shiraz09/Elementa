using System;
using System.Collections;
using UnityEngine;

public class ColumnClearAbility : BoardAbility
{
    public override IEnumerator Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        yield return board.StartCoroutine(board.ClearColumn(x));
       
    }
}