using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Bomb 3x3", fileName = "Bomb3x3Ability")]

public class Bomb3x3Ability : BoardAbility
{
    public override void Apply(Grid board, int x, int y, Grid.PieceType clickedType)
    {
        board.Bomb3x3(x, y);

    }
}