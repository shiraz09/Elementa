using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text movesText;
    public TMP_Text scoreText;

    // במקום 3 אימג'ים – אימג' בודד שמחליף Sprite לפי מצב
    public Image starsImage;

    // 4 ספרייטים: [0]=שלושה ריקים, [1]=כוכב אחד, [2]=שניים, [3]=שלושה מלאים
    public Sprite[] starStates; // size 4 באינספקטור

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
        grid = FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError("GameUI: Grid not found!");
            return;
        }

        if (starsImage != null)
        {
            starsImage.color = Color.white;
            starsImage.preserveAspect = true;
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (grid == null) return;

        UpdateMovesDisplay();
        UpdateScoreDisplay();
        UpdateStarsDisplay();
    }

    private void UpdateMovesDisplay()
    {
        if (movesText == null) return;

        movesText.text = $"Moves: {grid.currentMoves}";
        if (grid.currentMoves <= 5)       movesText.color = Color.red;
        else if (grid.currentMoves <= 10) movesText.color = Color.yellow;
        else                              movesText.color = Color.white;
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText == null) return;

        scoreText.text = $"Score: {grid.currentScore:N0}";
        if (grid.currentScore != lastScore)
        {
            StartCoroutine(AnimateScoreChange());
            lastScore = grid.currentScore;
        }
    }

    private void UpdateStarsDisplay()
    {
        if (starsImage == null) return;
        if (starStates == null || starStates.Length < 4)
        {
            Debug.LogWarning("GameUI: Please assign 4 star state sprites (0..3).");
            return;
        }

        int currentStars = Mathf.Clamp(grid.GetStarRating(), 0, 3);

        // החלפת הספרייט לפי מצב הכוכבים
        starsImage.sprite = starStates[currentStars];

        // אם בדיוק עלינו בדרגה – נאנמת קלות
        if (currentStars > lastStars)
            StartCoroutine(AnimateStarEarned());

        lastStars = currentStars;
    }

    private System.Collections.IEnumerator AnimateScoreChange()
    {
        if (scoreText == null) yield break;

        Vector3 originalScale = scoreText.transform.localScale;
        Color originalColor = scoreText.color;

        scoreText.transform.localScale = originalScale * 1.2f;
        scoreText.color = Color.green;

        yield return new WaitForSeconds(scoreAnimationDuration);

        scoreText.transform.localScale = originalScale;
        scoreText.color = originalColor;
    }

    private System.Collections.IEnumerator AnimateStarEarned()
    {
        if (starsImage == null) yield break;

        Vector3 originalScale = starsImage.transform.localScale;

        starsImage.transform.localScale = originalScale * 1.1f;

        float elapsed = 0f;
        while (elapsed < starAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(Mathf.Clamp01(elapsed / starAnimationDuration) * Mathf.PI);
            starsImage.transform.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t);
            yield return null;
        }

        starsImage.transform.localScale = originalScale;
    }

    public void ShowStarEarnedMessage(int starCount)
    {
        Debug.Log($"⭐ Star {starCount} earned! ⭐");
        if (messageDisplay != null) messageDisplay.ShowStarMessage(starCount);
    }

    public void ShowNoMovesWarning()
    {
        Debug.Log("⚠️ Warning: Only a few moves left!");
        if (messageDisplay != null) messageDisplay.ShowWarningMessage();
    }

    public void ShowLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"🎉 Level Complete! 🎉");
        Debug.Log($"Final Score: {finalScore:N0}");
        Debug.Log($"Stars Earned: {starsEarned}/3");
        if (messageDisplay != null) messageDisplay.ShowLevelCompleteMessage(finalScore, starsEarned);
    }
}