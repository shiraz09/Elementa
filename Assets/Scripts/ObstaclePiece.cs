using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class ObstaclePiece : MonoBehaviour
{
    public enum Kind { Grass, Ice }

    [Header("Setup")]
    public Kind kind;
    public int hitsToClear = 4;       // Grass=4, Ice=6
    public Sprite[] stageSprites;     // 3 ספרייטים לשלבים

    [Header("Hit FX (DOTween)")]
    public bool playHitFX = true;
    public float punchScale = 0.15f;
    public float fxDuration = 0.18f;
    public float colorFlashFactor = 1.2f;

    int hitsSoFar = 0;
    Image img;
    RectTransform rt;
    Color originalColor;
    Vector3 originalScale;

    void Awake()
    {
        img = GetComponent<Image>();
        rt  = GetComponent<RectTransform>();
        originalColor = img ? img.color : Color.white;
        originalScale = rt  ? rt.localScale : Vector3.one;
    }

    void OnDisable() { KillTweens(); }
    void OnDestroy() { KillTweens(); }

    public void KillTweens()
    {
        if (rt)  rt.DOKill();
        if (img) img.DOKill();
        DOTween.Kill(this); // במקרה וקישרנו סיקוונס ל־this כ־target
    }

    public bool Damage(int amount = 1)
    {
        hitsSoFar += amount;
        bool spriteChanged = UpdateSprite();
        if (playHitFX) PlayHitFX(spriteChanged);
        return hitsSoFar >= hitsToClear;
    }

    private bool UpdateSprite()
    {
        if (!img || stageSprites == null || stageSprites.Length == 0) return false;

        Sprite next = null;

        if (kind == Kind.Grass)
        {
            if (hitsSoFar >= 3) next = stageSprites[Mathf.Min(2, stageSprites.Length - 1)];
            else if (hitsSoFar >= 2 && stageSprites.Length >= 2) next = stageSprites[1];
            else if (hitsSoFar >= 1) next = stageSprites[0];
        }
        else // Ice
        {
            if (hitsSoFar >= 5 && stageSprites.Length >= 3) next = stageSprites[2];
            else if (hitsSoFar >= 4 && stageSprites.Length >= 2) next = stageSprites[1];
            else if (hitsSoFar >= 2) next = stageSprites[0];
        }

        if (next != null && img.sprite != next) { img.sprite = next; return true; }
        return false;
    }

    private void PlayHitFX(bool spriteChanged)
    {
        if (!isActiveAndEnabled || rt == null || img == null) return;

        float dur = spriteChanged ? fxDuration * 1.1f : fxDuration;
        float punch = spriteChanged ? punchScale * 1.15f : punchScale;

        KillTweens();                      // חשוב: לעצור כל טווין קודם
        rt.localScale = originalScale;
        img.color     = originalColor;

        var seq = DOTween.Sequence().SetTarget(this);

        seq.Join(rt.DOPunchScale(Vector3.one * punch, dur, 6, 0.8f));

        if (colorFlashFactor > 1f)
        {
            Color flash = originalColor * colorFlashFactor;
            flash.a = originalColor.a;
            seq.Join(img.DOColor(flash, dur * 0.5f))
               .Append(img.DOColor(originalColor, dur * 0.5f));
        }

        seq.OnComplete(() =>
        {
            if (rt)  rt.localScale = originalScale;
            if (img) img.color     = originalColor;
        });
    }

    public void ResetState()
    {
        hitsSoFar = 0;
        if (img) img.color = originalColor;
        if (rt)  rt.localScale = originalScale;
        UpdateSprite();
    }
}