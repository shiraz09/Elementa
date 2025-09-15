using UnityEngine;

public abstract class BoardAbility
{
    public abstract void Apply(Grid board, int x, int y, Grid.PieceType type);
}