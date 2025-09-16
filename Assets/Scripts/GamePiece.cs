using UnityEngine;
using UnityEngine.EventSystems;

public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private int x;
    private int y;
    public  int score;

    private Grid.PieceType type;
    private Grid grid;

    private MoveablePiece  moveableComponent;

    private ClearablePiece clearableComponent;

    public MoveablePiece  MoveableComponent  => moveableComponent;

    public ClearablePiece ClearableComponent => clearableComponent;

    // swipe tracking
    private Vector2 pressScreenPos;
    private const float SWIPE_THRESHOLD = 12f; // pixels

    void Awake()
    {
        moveableComponent  = GetComponent<MoveablePiece>();

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

    public Grid.PieceType Type => type;
    public Grid GridRef => grid;

    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;

        if (moveableComponent == null)
            moveableComponent = GetComponent<MoveablePiece>();
        if (moveableComponent == null)
            moveableComponent = gameObject.AddComponent<MoveablePiece>();

        moveableComponent.Init(_grid);
    }

    // Input
    public void OnPointerDown(PointerEventData e)
    {
        pressScreenPos = e.position;
        grid.PressPiece(this);
    }

    public void OnPointerUp(PointerEventData e)
    {
        Vector2 delta = e.position - pressScreenPos;

        // tap or too short swipe → cancel
        if (delta.magnitude < SWIPE_THRESHOLD)
        {
            grid.ReleasePiece();
            return;
        }

        int dx = 0, dy = 0;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dx = delta.x > 0 ? 1 : -1;   // right/left
        else
            dy = delta.y > 0 ? 1 : 1;   // up/down (screen coords)

        grid.TrySwapInDirection(this, dx, dy);
    }

    // Obstacles are not moveable
    public bool IsMoveable()
    {
        return type != Grid.PieceType.ICEOBS
            && type != Grid.PieceType.GRASSOBS
            && moveableComponent != null;
    }

    public bool IsClearable() => clearableComponent != null;
}