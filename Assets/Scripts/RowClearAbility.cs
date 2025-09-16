using System.Collections;
using UnityEngine;

public class RowClearAbility : BoardAbility
{
    public override IEnumerator Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        yield return board.StartCoroutine(board.ClearRow(y));
    }
}
