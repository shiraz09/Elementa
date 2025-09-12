using UnityEngine;

public class GamePiece : MonoBehaviour
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
    void Awake()
    {
        moveableComponent = GetComponent<MoveablePiece>();
        colorComponent = GetComponent<ColorPiece>();
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
    void OnMouseEnter()
    {
        grid.EnterPiece(this);
    }
    void OnMouseDown()
    {
        grid.PressPiece(this);
            
    }
    void OnMouseUp()
    {
        grid.ReleasePiece();
        
    }
    public bool IsMoveable()
    {
        return moveableComponent != null;
    }
    public bool IsColored(){
        return colorComponent != null;
    }
    
}
