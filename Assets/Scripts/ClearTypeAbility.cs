using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Clear All Of Type", fileName = "ClearTypeAbility")]

public class ClearTypeAbility : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.ClearAllOfType(clickedType);

    }
}