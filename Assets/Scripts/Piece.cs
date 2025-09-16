using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour
{
    public int x, y;
    private GamePiece gp;

    private Grid board;

    public void Init(Grid b, int _x, int _y, Grid.PieceType _type)
    {
        board = b;
        gp = GetComponent<GamePiece>();
        gp.Init(_x, _y, b, _type);
       
    }
}
