using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GamePiece))]
public class MoveablePiece : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveDuration = 0.25f;
    public Ease  moveEase     = Ease.InOutQuad;

    [Header("Landing Punch FX")]
    public bool  punchOnEnd   = true;
    public float punchAmount  = 0.1f;
    public float punchTime    = 0.18f;
    public int   punchVibrato = 1;
    public float punchElastic = 0.5f;

    RectTransform rt;
    Grid          grid;
    GamePiece     piece;

    void Awake()
    {
        rt    = GetComponent<RectTransform>();
        piece = GetComponent<GamePiece>();
    }

    public void Init(Grid g)
    {
        grid = g;
    }

    public void Move(int newX, int newY)
    {
        if (grid == null || rt == null) return;

        if (piece != null) { piece.X = newX; piece.Y = newY; }
        Vector2 target = grid.GetUIPos(newX, newY);

        // להרוג טווינים קודמים ולהביא לקדמת ההיררכיה
        rt.DOKill();
        rt.SetAsLastSibling();

        rt.DOAnchorPos(target, moveDuration)
        .SetEase(moveEase)
        .SetUpdate(false) // משתמש בזמן המשחק (אם תרצי שיעבוד גם כשהמשחק על Pause: SetUpdate(true))
        .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy)
        .OnComplete(() =>
        {
            if (!punchOnEnd || rt == null) return;
            rt.DOPunchScale(Vector3.one * punchAmount, punchTime, punchVibrato, punchElastic)
                .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
        });
    }

    public void KillTweens()
    {
        if (rt != null) rt.DOKill();
    }

    void OnDisable() { KillTweens(); }
    void OnDestroy() { KillTweens(); }
}