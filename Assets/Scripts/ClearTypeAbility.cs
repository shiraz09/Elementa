using System.Collections;
using UnityEngine;

public class ClearTypeAbility : BoardAbility
{
    public override IEnumerator Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        yield return board.StartCoroutine(board.ClearAllOfType(clickedType));

    }
}