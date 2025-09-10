using UnityEngine;
using UnityEngine.UI;   // נדרש בשביל Image
using System.Collections.Generic;

public class ColorPiece : MonoBehaviour
{
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;   // סוג הצבע
        public Sprite sprite;     // הספרייט שמתאים לצבע
    }

    public enum ColorType
    {
        GREEN,
        YELLOW,
        BROWN,
        BLUE,
        ANY,
        COUNT
    };

    [Header("Color sprites mapping")]
    public ColorSprite[] colorSprites;

    private ColorType color;
    private Image image;   // במקום SpriteRenderer
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    void Awake()
    {
        // נוודא שיש לנו Image על האובייקט
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("ColorPiece: Missing UI Image on " + gameObject.name);
        }
        // בניית מילון מיפוי צבע→ספרייט
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        foreach (var entry in colorSprites)
        {
            if (entry.sprite != null && !colorSpriteDict.ContainsKey(entry.color))
            {
                colorSpriteDict.Add(entry.color, entry.sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;

        if (image != null && colorSpriteDict.ContainsKey(newColor))
        {
            image.sprite = colorSpriteDict[newColor];
        }
        else
        {
            Debug.LogWarning($"ColorPiece: No sprite assigned for {newColor} on {name}");
        }
    }

    public int NumColors
    {
        get { return colorSprites.Length; }
    }

    public void SetRandomColor()
    {
        int rand = Random.Range(0, (int)ColorType.COUNT);
        SetColor((ColorType)rand);
    }
}