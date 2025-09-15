using UnityEngine;

public class ClearTypeAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearAllOfType(clickedType);

    }
}