using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text scoreText;

    // כוכבים – תמונה אחת שמחליפה ספרייט
    [SerializeField] private Image starsImage;

    // 4 ספרייטים: [0]=0 כוכבים, [1]=⭐, [2]=⭐⭐, [3]=⭐⭐⭐
    [SerializeField] private Sprite[] starStates = new Sprite[4];

    [Header("Goals UI")]
    [SerializeField] private RectTransform goalsContainer; // עם VerticalLayoutGroup
    [SerializeField] private GameObject goalRowPrefab;     // פריפאב עם TMP_Text

    [Header("Animation Settings")]
    [SerializeField] private float starAnimationDuration = 0.5f;
    [SerializeField] private float scoreAnimationDuration = 0.3f;

    [Header("Message Display (optional)")]
    [SerializeField] private MessageDisplay messageDisplay; // אם יש אצלך

    private Grid grid;
    private readonly List<TMP_Text> goalLabels = new();

    private int lastScore = -1;
    private int lastStars = -1;

    private Coroutine scoreAnimCo;
    private Coroutine starAnimCo;

    private void Awake()
    {

    #if UNITY_2023_1_OR_NEWER
        if (grid == null) grid = FindFirstObjectByType<Grid>();   // מהיר ומומלץ
    #else
        if (grid == null) grid = FindObjectOfType<Grid>();        // תאימות לאחור
    #endif

        if (starsImage != null)
        {
            starsImage.color = Color.white;
            starsImage.preserveAspect = true;
        }
    }

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("GameUI: Grid not found in scene.");
            enabled = false;
            return;
        }

        UpdateUI(force: true);
    }

    private void Update()
    {
        UpdateUI();
    }

    // ---------- Public API ----------

    public void ShowGoals(List<CollectGoal> goals)
    {
        if (!goalsContainer || !goalRowPrefab) return;

        foreach (Transform t in goalsContainer) Destroy(t.gameObject);
        goalLabels.Clear();

        for (int i = 0; i < goals.Count; i++)
        {
            var row = Instantiate(goalRowPrefab, goalsContainer);
            var label = row.GetComponentInChildren<TMP_Text>();
            if (!label)
            {
                Debug.LogWarning("GameUI: goalRowPrefab must contain a TMP_Text.");
                continue;
            }
            goalLabels.Add(label);
        }

        UpdateGoals(goals);
    }

    public void UpdateGoals(List<CollectGoal> goals)
    {
        if (goals == null) return;

        int count = Mathf.Min(goals.Count, goalLabels.Count);
        for (int i = 0; i < count; i++)
        {
            var g = goals[i];
            var label = goalLabels[i];
            if (label) label.text = $"{g.type}: {g.current}/{g.target}";
        }
    }

    public void UpdateUI(bool force = false)
    {
        if (grid == null) return;

        UpdateMovesDisplay();
        UpdateScoreDisplay(force);
        UpdateStarsDisplay(force);
    }

    public void ShowStarEarnedMessage(int starCount)
    {
        Debug.Log($"⭐ Star {starCount} earned! ⭐");
        messageDisplay?.ShowStarMessage(starCount);
    }

    public void ShowNoMovesWarning()
    {
        Debug.Log("⚠️ Warning: Only a few moves left!");
        messageDisplay?.ShowWarningMessage();
    }

    public void ShowLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"🎉 Level Complete! Score={finalScore:N0}, Stars={starsEarned}/3");
        messageDisplay?.ShowLevelCompleteMessage(finalScore, starsEarned);
    }

    // ---------- Internals ----------

    private void UpdateMovesDisplay()
    {
        if (!movesText) return;

        movesText.text = $"Moves: {grid.currentMoves}";

        if (grid.currentMoves <= 5)       movesText.color = Color.red;
        else if (grid.currentMoves <= 10) movesText.color = Color.yellow;
        else                              movesText.color = Color.white;
    }

    private void UpdateScoreDisplay(bool force)
    {
        if (!scoreText) return;

        int cur = grid.currentScore;
        scoreText.text = $"Score: {cur:N0}";

        if (force || lastScore < 0) { lastScore = cur; return; }

        if (cur != lastScore)
        {
            if (scoreAnimCo != null) StopCoroutine(scoreAnimCo);
            scoreAnimCo = StartCoroutine(AnimateScoreChange());
            lastScore = cur;
        }
    }

    private void UpdateStarsDisplay(bool force)
    {
        if (!starsImage) return;
        if (starStates == null || starStates.Length < 4) return;

        int curStars = Mathf.Clamp(grid.GetStarRating(), 0, 3);

        if (force || lastStars < 0)
        {
            starsImage.sprite = starStates[curStars];
            lastStars = curStars;
            return;
        }

        if (curStars != lastStars)
        {
            starsImage.sprite = starStates[curStars];

            if (curStars > lastStars)
            {
                if (starAnimCo != null) StopCoroutine(starAnimCo);
                starAnimCo = StartCoroutine(AnimateStarEarned());
            }

            lastStars = curStars;
        }
    }

    private IEnumerator AnimateScoreChange()
    {
        var tr = scoreText.transform;
        Vector3 baseScale = tr.localScale;
        Color baseColor = scoreText.color;

        tr.localScale = baseScale * 1.2f;
        scoreText.color = Color.green;

        float t = 0f;
        while (t < scoreAnimationDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / scoreAnimationDuration);
            tr.localScale = Vector3.Lerp(baseScale * 1.2f, baseScale, k);
            yield return null;
        }

        tr.localScale = baseScale;
        scoreText.color = baseColor;
    }

    private IEnumerator AnimateStarEarned()
    {
        var tr = starsImage.transform;
        Vector3 baseScale = tr.localScale;

        float t = 0f;
        while (t < starAnimationDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / starAnimationDuration) * Mathf.PI);
            tr.localScale = Vector3.Lerp(baseScale, baseScale * 1.1f, k);
            yield return null;
        }

        tr.localScale = baseScale;
    }

    // מאפשר הזרקת Grid ידנית אם תרצי
    public void SetGrid(Grid g)
    {
        grid = g;
        UpdateUI(force: true);
    }
}