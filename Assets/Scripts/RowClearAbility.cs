using UnityEngine;

public class RowClearAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearRow(y);
    }
}
