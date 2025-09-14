using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text movesText; // טקסט מהלכים
    public TMP_Text scoreText;
    public Image[] starImages; // 3 תמונות כוכבים
    public Image starEmpty; // כוכב ריק (אובייקט תמונה)
    
    [Header("Animation Settings")]
    public float starAnimationDuration = 0.5f;
    public float scoreAnimationDuration = 0.3f;
    
    [Header("Message Display")]
    public MessageDisplay messageDisplay;
    
    private Grid grid;
    private int lastScore = 0;
    private int lastStars = 0;
    
    void Start()
    {
        // מציאת Grid במשחק
        grid = FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError("GameUI: Grid not found!");
            return;
        }
        
        // אתחול UI
        UpdateUI();
    }
    
    void Update()
    {
        // עדכון UI כל פריים (למקרה של שינויים)
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (grid == null) return;
        
        // עדכון מהלכים
        UpdateMovesDisplay();
        
        // עדכון ניקוד
        UpdateScoreDisplay();
        
        // עדכון כוכבים
        UpdateStarsDisplay();
    }
    
    private void UpdateMovesDisplay()
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {grid.currentMoves}";
            
            // שינוי צבע אם נשארו מעט מהלכים
            if (grid.currentMoves <= 5)
            {
                movesText.color = Color.red;
            }
            else if (grid.currentMoves <= 10)
            {
                movesText.color = Color.yellow;
            }
            else
            {
                movesText.color = Color.white;
            }
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {grid.currentScore:N0}"; // N0 מוסיף פסיקים למספרים גדולים
            
            // אנימציה של הניקוד אם הוא השתנה
            if (grid.currentScore != lastScore)
            {
                StartCoroutine(AnimateScoreChange());
                lastScore = grid.currentScore;
            }
        }
    }
    
    private void UpdateStarsDisplay()
    {
        if (starImages == null || starImages.Length != 3) return;
        
        int currentStars = grid.GetStarRating();
        
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                // כוכב מלא אם השגנו אותו, ריק אחרת
                if (i < currentStars)
                {
                    // כוכב מלא - השתמש בספרייט של הכוכב עצמו
                    starImages[i].sprite = starImages[i].sprite; // נשאר כמו שהוא
                }
                else
                {
                    // כוכב ריק - השתמש בספרייט של starEmpty
                    starImages[i].sprite = starEmpty.sprite;
                }
                
                // אנימציה של כוכב חדש
                if (i < currentStars && i >= lastStars)
                {
                    StartCoroutine(AnimateStarEarned(i));
                }
            }
        }
        
        lastStars = currentStars;
    }
    
    // אנימציה של שינוי ניקוד
    private System.Collections.IEnumerator AnimateScoreChange()
    {
        if (scoreText == null) yield break;
        
        Vector3 originalScale = scoreText.transform.localScale;
        Color originalColor = scoreText.color;
        
        // הגדלה וצבע ירוק
        scoreText.transform.localScale = originalScale * 1.2f;
        scoreText.color = Color.green;
        
        yield return new WaitForSeconds(scoreAnimationDuration);
        
        // חזרה לגודל וצבע רגילים
        scoreText.transform.localScale = originalScale;
        scoreText.color = originalColor;
    }
    
    // אנימציה של כוכב חדש
    private System.Collections.IEnumerator AnimateStarEarned(int starIndex)
    {
        if (starImages == null || starIndex >= starImages.Length || starImages[starIndex] == null) 
            yield break;
        
        Image starImage = starImages[starIndex];
        Vector3 originalScale = starImage.transform.localScale;
        
        // הגדלה
        starImage.transform.localScale = originalScale * 1.5f;
        
        // אנימציה של סיבוב
        float elapsed = 0f;
        while (elapsed < starAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float rotation = (elapsed / starAnimationDuration) * 360f;
            starImage.transform.rotation = Quaternion.Euler(0, 0, rotation);
            
            yield return null;
        }
        
        // חזרה לגודל וסיבוב רגילים
        starImage.transform.localScale = originalScale;
        starImage.transform.rotation = Quaternion.identity;
    }
    
    // פונקציה להצגת הודעה על כוכב חדש
    public void ShowStarEarnedMessage(int starCount)
    {
        Debug.Log($"⭐ Star {starCount} earned! ⭐");
        
        if (messageDisplay != null)
        {
            messageDisplay.ShowStarMessage(starCount);
        }
    }
    
    // פונקציה להצגת הודעה על מהלכים נגמרים
    public void ShowNoMovesWarning()
    {
        Debug.Log("⚠️ Warning: Only a few moves left!");
        
        if (messageDisplay != null)
        {
            messageDisplay.ShowWarningMessage();
        }
    }
    
    // פונקציה להצגת תוצאות סיום שלב
    public void ShowLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"🎉 Level Complete! 🎉");
        Debug.Log($"Final Score: {finalScore:N0}");
        Debug.Log($"Stars Earned: {starsEarned}/3");
        
        if (messageDisplay != null)
        {
            messageDisplay.ShowLevelCompleteMessage(finalScore, starsEarned);
        }
    }
}
