using UnityEngine;

public class MoveablePiece : MonoBehaviour
{
    private GamePiece piece;
    private RectTransform rect;


    void Awake()
    {
        piece = GetComponent<GamePiece>();
        rect = GetComponent<RectTransform>();
        
    }
    public void Move(int newX, int newY)
    {
        piece.X = newX;
        piece.Y = newY;
        RectTransform rt = (RectTransform)piece.transform;
        rt.anchoredPosition = piece.GridRef.GetUIPos(newX, newY);
    }
}