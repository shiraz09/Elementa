using UnityEngine;
using UnityEngine.EventSystems;

public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    private int x;
    private int y;
    public  int score;

    private Grid.PieceType type;
    private Grid grid;

    private MoveablePiece  moveableComponent;
    private ColorPiece     colorComponent;
    private ClearablePiece clearableComponent;

    public MoveablePiece  MoveableComponent  => moveableComponent;
    public ColorPiece     ColorComponent     => colorComponent;
    public ClearablePiece ClearableComponent => clearableComponent;

    void Awake()
    {
        moveableComponent  = GetComponent<MoveablePiece>();   // שימי לב: זה הקומפוננט היחיד שמזיז
        colorComponent     = GetComponent<ColorPiece>();
        clearableComponent = GetComponent<ClearablePiece>();
    }

    public int X
    {
        get => x;
        set { if (IsMoveable()) x = value; }
    }

    public int Y
    {
        get => y;
        set { if (IsMoveable()) y = value; }
    }

    public Grid.PieceType Type   => type;
    public Grid           GridRef => grid;

    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;

        // לוודא שיש MoveablePiece ושהוא מאותחל עם ה-Grid
        if (moveableComponent == null)
            moveableComponent = GetComponent<MoveablePiece>();

        moveableComponent?.Init(_grid);
    }

    // קלט עכבר/מגע
    public void OnPointerDown (PointerEventData e) => grid.PressPiece(this);
    public void OnPointerEnter(PointerEventData e) => grid.EnterPiece(this);
    public void OnPointerUp   (PointerEventData e) => grid.ReleasePiece();

    // מכשולים לא ניתנים להזזה
    public bool IsMoveable()
    {
        return type != Grid.PieceType.ICEOBS
            && type != Grid.PieceType.GRASSOBS
            && moveableComponent != null;
    }

    public bool IsColored()   => colorComponent != null;
    public bool IsClearable() => clearableComponent != null;
}