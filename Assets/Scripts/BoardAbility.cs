using System.Collections;
using UnityEngine;

public abstract class BoardAbility
{
    public abstract IEnumerator Apply(Grid board, int x, int y, Grid.PieceType type);
}