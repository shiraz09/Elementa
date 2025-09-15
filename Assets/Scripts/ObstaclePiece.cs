using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ObstaclePiece : MonoBehaviour
{
    public enum Kind { Grass, Ice }

    [Header("Setup")]
    public Kind kind;

    [Tooltip("כמה פגיעות עד שהמכשול נעלם")]
    public int hitsToClear = 4;  // Grass=4, Ice=6

    [Tooltip("ספרייטים מדורגים (לא כוללים את הספרייט ההתחלתי שעל ה-Image)")]
    public Sprite[] stageSprites; // לדשא 3; לקרח 3

    private int hitsSoFar = 0;
    private Image img;

    void Awake() { img = GetComponent<Image>(); }

    // מוסיף נזק; מחזיר true אם הושמד
    public bool Damage(int amount = 1)
    {
        hitsSoFar += amount;
        UpdateSprite();
        return hitsSoFar >= hitsToClear;
    }

    private void UpdateSprite()
    {
        if (!img || stageSprites == null || stageSprites.Length == 0) return;

        if (kind == Kind.Grass)
        {
            // דשא: מתעדכן בכל פגיעה (1/2/3) וב-4 נעלם
            if (hitsSoFar >= 3) img.sprite = stageSprites[Mathf.Min(2, stageSprites.Length - 1)];
            else if (hitsSoFar >= 2 && stageSprites.Length >= 2) img.sprite = stageSprites[1];
            else if (hitsSoFar >= 1) img.sprite = stageSprites[0];
        }
        else // Ice
        {
            // קרח: משתנה כל 2 פגיעות; יש לך 3 תמונות → 0: אחרי 2, 1: אחרי 4, 2: כמעט שבור (5)
            if (hitsSoFar >= 5 && stageSprites.Length >= 3) img.sprite = stageSprites[2];
            else if (hitsSoFar >= 4 && stageSprites.Length >= 2) img.sprite = stageSprites[1];
            else if (hitsSoFar >= 2) img.sprite = stageSprites[0];
        }
    }

    public void ResetState() { hitsSoFar = 0; } // לשימוש עתידי אם תרצי
}