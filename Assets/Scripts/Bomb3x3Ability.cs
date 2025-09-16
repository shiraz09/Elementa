using System.Collections;
using UnityEngine;

public class Bomb3x3Ability : BoardAbility
{
    public override IEnumerator Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        yield return board.StartCoroutine(board.Bomb3x3(x, y));
    }
}