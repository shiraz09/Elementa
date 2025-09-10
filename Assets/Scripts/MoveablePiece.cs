using UnityEngine;

public class MoveablePiece : MonoBehaviour
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
        piece.transform.localPosition = piece.GridRef.GetScreenPosition(newX,newY);
    }
}