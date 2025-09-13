using UnityEngine;
using UnityEngine.EventSystems;


public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    private int x;
    private int y;
    private Grid.PieceType type;
    private Grid grid;
    private MoveablePiece moveableComponent;

    public MoveablePiece MoveableComponent
    {
        get { return moveableComponent; }
    }
    private ColorPiece colorComponent;

    public ColorPiece ColorComponent
    {
        get { return colorComponent; }
    }
    private ClearablePiece clearableComponent;
    public ClearablePiece ClearableComponent
    {
        get { return clearableComponent; }
    }
    void Awake()
    {
        moveableComponent = GetComponent<MoveablePiece>();
        colorComponent = GetComponent<ColorPiece>();
        clearableComponent = GetComponent<ClearablePiece>();
    }
    public int X
    {
        get { return x; }
        set
        {
            if (IsMoveable())
            {
                x = value;
            }
        }
    }
    public int Y
    {
        get { return y; }
        set
        {
            if (IsMoveable())
            {
                y = value;
            }
        }
    }
    public Grid.PieceType Type
    {
        get { return type; }
    }
    public Grid GridRef
    {
        get { return grid; }
    }
    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;

    }
    public void OnPointerDown(PointerEventData e)
    {
        grid.PressPiece(this);
    }
    public void OnPointerEnter(PointerEventData e)
    {
        grid.EnterPiece(this);
    }
    public void OnPointerUp(PointerEventData e)
    {
        grid.ReleasePiece();
    }


    public bool IsMoveable()
    {
        return moveableComponent != null;
    }
    public bool IsColored()
    {
        return colorComponent != null;
    }
    public bool IsClearable()
    {
        return clearableComponent != null;
    }
    
}
