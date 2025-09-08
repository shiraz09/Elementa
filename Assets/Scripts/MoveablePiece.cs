using UnityEngine;

public class MoveablePiece : MonoBehavior
{
    private GamePiece piece;

    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }
    public void Move(int newX, int newY)
    {
        piece.X = newX;
        piece.Y = newY;
        piece.transform.localPosition = piece.GridRef.getWorldPosition(newX, newY);
    }
}