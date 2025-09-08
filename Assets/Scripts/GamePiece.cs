using UnityEngine;

public class GamePiece : MonoBehavior{
    private int x;
    private int y;
    private Grid.PieceType type;
    private Grid grid;
    private MoveablePiece moveableComponent;

    public MoveablePiece MoveableComponent
    {
        get{ return moveableComponent; }
    }
    void Awake()
    {
        moveableComponent = GetComponent<MoveablePiece>();
    }
    public int X {
        get { return x; }
        set{
            if (IsMoveable()){
                x = value; }
        }
    }
    public int Y {
        get { return y; }
        set{ if (IsMoveable()){
                y = value;}
        }
    }
    public Grid.PieceType Type{
        get { return type; }
    }
    public Grid GridRef {
        get { return grid; }
    }
    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type) {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
        
    }
    public bool IsMoveable(){
        return moveableComponent != null;}
    
}
