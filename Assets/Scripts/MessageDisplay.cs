using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MessageDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject messagePanel;
    public TMP_Text messageText;
    public Image messageBackground;
    
    [Header("Animation Settings")]
    public float messageDuration = 2f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    
    [Header("Message Types")]
    public Color starMessageColor = Color.yellow;
    public Color warningMessageColor = Color.red;
    public Color scoreMessageColor = Color.green;
    public Color levelCompleteColor = Color.cyan;
    
    private Coroutine currentMessageCoroutine;
    
    void Start()
    {
        // הסתרת הפאנל בהתחלה
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    
    // הצגת הודעה על כוכב חדש
    public void ShowStarMessage(int starCount)
    {
        string message = $"⭐ Star {starCount} Earned! ⭐";
        ShowMessage(message, starMessageColor);
    }
    
    // הצגת הודעה על מהלכים נגמרים
    public void ShowWarningMessage()
    {
        string message = "⚠️ Warning: Few moves left!";
        ShowMessage(message, warningMessageColor);
    }
    
    // הצגת הודעה על ניקוד
    public void ShowScoreMessage(int score)
    {
        string message = $"+{score:N0} Points!";
        ShowMessage(message, scoreMessageColor);
    }
    
    // הצגת הודעה על סיום שלב
    public void ShowLevelCompleteMessage(int score, int stars)
    {
        string message = $"🎉 Level Complete! 🎉\nScore: {score:N0}\nStars: {stars}/3";
        ShowMessage(message, levelCompleteColor, 4f); // הודעה ארוכה יותר
    }
    
    // הצגת הודעה כללית
    public void ShowMessage(string message, Color color, float duration = -1)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        
        currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message, color, duration));
    }
    
    private IEnumerator DisplayMessageCoroutine(string message, Color color, float customDuration)
    {
        float duration = customDuration > 0 ? customDuration : messageDuration;
        
        // הצגת הפאנל
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
        
        // הגדרת הטקסט והצבע
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }
        
        if (messageBackground != null)
        {
            messageBackground.color = new Color(color.r, color.g, color.b, 0.3f);
        }
        
        // אנימציה של הופעה
        yield return StartCoroutine(FadeIn());
        
        // המתנה
        yield return new WaitForSeconds(duration);
        
        // אנימציה של היעלמות
        yield return StartCoroutine(FadeOut());
        
        // הסתרת הפאנל
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        
        currentMessageCoroutine = null;
    }
    
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        CanvasGroup canvasGroup = messagePanel?.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = messagePanel?.AddComponent<CanvasGroup>();
        }
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeInDuration;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        CanvasGroup canvasGroup = messagePanel?.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = messagePanel?.AddComponent<CanvasGroup>();
        }
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeOutDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    // פונקציה להפסקת הודעה נוכחית
    public void StopCurrentMessage()
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
}
