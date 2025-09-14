using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Row Clear", fileName = "RowClearAbility")]
public class RowClearAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearRow(y);

    }
}
