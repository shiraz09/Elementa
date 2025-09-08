using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IPointerClickHandler
{
    public int x, y;
    public Grid.PieceType type;
    private Grid board;

    public void Init(Grid b, int _x, int _y, Grid.PieceType _type)
    {
        board = b;
        x = _x;
        y = _y;
        type = _type;
    }
    public void OnPointerClick(PointerEventData eventData) {
        board.OnCellClicked(x, y, type);
    }
}
