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

    [Header("Stars")]
    [SerializeField] private Image starsImage;
    [SerializeField] private Sprite[] starStates = new Sprite[4];

    [Header("Goals UI")]
    [SerializeField] private RectTransform goalsContainer;
    [SerializeField] private GameObject goalRowPrefab;

    [Header("Piece Icons (mapping)")]
    [SerializeField] private List<PieceIcon> pieceIcons = new List<PieceIcon>();

    [Header("Animation Settings")]
    [SerializeField] private float starAnimationDuration = 0.5f;
    [SerializeField] private float scoreAnimationDuration = 0.3f;

    [Header("Message Display (optional)")]
    [SerializeField] private MessageDisplay messageDisplay;

    private Grid grid;
    private readonly List<GoalUIItem> goalItems = new List<GoalUIItem>();
    private Dictionary<Grid.PieceType, Sprite> iconMap;

    private int lastScore = -1;
    private int lastStars = -1;
    private Coroutine scoreAnimCo, starAnimCo;

    private void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        grid = FindFirstObjectByType<Grid>();
#else
        grid = FindObjectOfType<Grid>();
#endif
        if (starsImage != null)
        {
            starsImage.color = Color.white;
            starsImage.preserveAspect = true;
        }

        iconMap = new Dictionary<Grid.PieceType, Sprite>();
        foreach (var p in pieceIcons)
        {
            if (!iconMap.ContainsKey(p.type) && p.sprite != null)
                iconMap.Add(p.type, p.sprite);
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

        if (goalsContainer != null && goalItems.Count == 0)
        {
            goalItems.Clear();
            var found = goalsContainer.GetComponentsInChildren<GoalUIItem>(true);
            goalItems.AddRange(found);
        }

        UpdateUI(force: true);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void ShowGoals(List<CollectGoal> goals)
    {
        if (goalsContainer == null) return;

        bool hasGoals = goals != null && goals.Count > 0;

        // הסתר/הצג את כל הקונטיינר לפי מצב המשימות
        goalsContainer.gameObject.SetActive(hasGoals);

        if (!hasGoals)
        {
            // אין משימות: לכבות ילדים קיימים אם יש
            foreach (Transform t in goalsContainer)
                t.gameObject.SetActive(false);
            return;
        }

        if (goalRowPrefab != null)
        {
            foreach (Transform t in goalsContainer) Destroy(t.gameObject);
            goalItems.Clear();

            for (int i = 0; i < goals.Count; i++)
            {
                var go = Instantiate(goalRowPrefab, goalsContainer);
                var item = go.GetComponentInChildren<GoalUIItem>(true);
                if (item != null) goalItems.Add(item);
            }
        }
        else
        {
            if (goalItems.Count == 0)
            {
                var found = goalsContainer.GetComponentsInChildren<GoalUIItem>(true);
                goalItems.AddRange(found);
            }

            for (int i = 0; i < goalItems.Count; i++)
                goalItems[i].gameObject.SetActive(i < goals.Count);
        }

        UpdateGoals(goals);
    }

    public void UpdateGoals(List<CollectGoal> goals)
    {
        if (goalsContainer == null) return;

        bool hasGoals = goals != null && goals.Count > 0;
        goalsContainer.gameObject.SetActive(hasGoals);
        if (!hasGoals) return;

        int count = Mathf.Min(goals.Count, goalItems.Count);
        for (int i = 0; i < count; i++)
        {
            var g = goals[i];
            var item = goalItems[i];

            Sprite s = null;
            iconMap?.TryGetValue(g.type, out s);
            item.UpdateGoal(s, g.current, g.target);
            item.gameObject.SetActive(true);
        }

        for (int i = count; i < goalItems.Count; i++)
            goalItems[i].gameObject.SetActive(false);
    }

    public void ShowStarEarnedMessage(int starCount)
    {
        Debug.Log($"⭐ Star {starCount} earned!");
        messageDisplay?.ShowStarMessage(starCount);
    }

    public void ShowNoMovesWarning()
    {
        Debug.Log("⚠️ Only a few moves left!");
        messageDisplay?.ShowWarningMessage();
    }

    public void ShowLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"🎉 Level Complete! Score={finalScore:N0}, Stars={starsEarned}/3");
        messageDisplay?.ShowLevelCompleteMessage(finalScore, starsEarned);
    }

    public void SetGrid(Grid g)
    {
        grid = g;
        UpdateUI(force: true);
    }

    public void UpdateUI(bool force = false)
    {
        if (grid == null) return;

        UpdateMovesDisplay();
        UpdateScoreDisplay(force);
        UpdateStarsDisplay(force);
    }

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
        if (!starsImage || starStates == null || starStates.Length < 4) return;

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

    [System.Serializable]
    public class PieceIcon
    {
        public Grid.PieceType type;
        public Sprite sprite;
    }
}