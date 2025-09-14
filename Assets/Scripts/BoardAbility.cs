using UnityEngine;

public abstract class BoardAbility : ScriptableObject
{
    public string title;
    public string description;
    public Sprite icon;
    public Cost price;

    public abstract void Apply(Grid board, int x, int y, Grid.PieceType type);
}