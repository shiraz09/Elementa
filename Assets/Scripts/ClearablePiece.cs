using UnityEngine;
using System.Collections;  // נדרש עבור IEnumerator

public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;
    private bool isBeingCleared = false;
    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    protected GamePiece piece;

    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    public void Clear()
    {
        piece.GridRef.level.OnPieceCleared(piece);
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());  // תיקון איות
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.Play(clearAnimation.name);                 
            yield return new WaitForSeconds(clearAnimation.length); 
            Destroy(gameObject);
        }
    }
}