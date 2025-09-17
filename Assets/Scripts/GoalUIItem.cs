using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [Header("Refs")]
    public Image icon;
    public TMP_Text amountText;

    /// <summary>
    /// מעדכן את האייקון ואת הטקסט "נאסף/נדרש".
    /// אם אין ספרייט — נשאיר את האייקון כפי שהוא (או נכבה).
    /// </summary>
    public void UpdateGoal(Sprite sprite, int current, int target)
    {
        if (icon != null && sprite != null)
        {
            icon.sprite = sprite;
            icon.enabled = true;
            icon.preserveAspect = true;
        }
        else if (icon != null && sprite == null)
        {
            // אין אייקון רלוונטי — אפשר להשאיר כבוי
            icon.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = $"{current}/{target}";
        }
    }
}