using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text movesText;
    public TMP_Text scoreText;
    public Image[] starImages;   // exactly 3 Image slots in Inspector
    public Sprite starFilledSprite; // assign: the "full" star sprite
    public Sprite starEmptySprite;  // assign: the "empty/gray" star sprite

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

        // sanity: make sure stars are visible and use white color
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].color = Color.white;   // no accidental tint
                    starImages[i].raycastTarget = false; // שלא יחסום את הלוח
                    starImages[i].enabled = true;        // make sure it's on
                }
            }
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
        if (starImages == null || starImages.Length != 3) return;
        if (starFilledSprite == null || starEmptySprite == null)
        {
            Debug.LogWarning("GameUI: Please assign starFilledSprite and starEmptySprite in the Inspector.");
            return;
        }

        int currentStars = grid.GetStarRating(); // 0..3

        for (int i = 0; i < starImages.Length; i++)
        {
            var img = starImages[i];
            if (img == null) continue;

            // choose the correct sprite
            img.sprite = (i < currentStars) ? starFilledSprite : starEmptySprite;

            // optional: keep size consistent with the sprite
            // img.SetNativeSize();

            // play animation exactly when a star was just earned
            if (i < currentStars && i >= lastStars)
            {
                StartCoroutine(AnimateStarEarned(i));
            }
        }

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

    private System.Collections.IEnumerator AnimateStarEarned(int starIndex)
    {
        if (starImages == null || starIndex >= starImages.Length) yield break;
        var starImage = starImages[starIndex];
        if (starImage == null) yield break;

        Vector3 originalScale = starImage.transform.localScale;

        starImage.transform.localScale = originalScale * 1.5f;

        float elapsed = 0f;
        while (elapsed < starAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float rotation = (elapsed / starAnimationDuration) * 360f;
            starImage.transform.rotation = Quaternion.Euler(0, 0, rotation);
            yield return null;
        }

        starImage.transform.localScale = originalScale;
        starImage.transform.rotation = Quaternion.identity;
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