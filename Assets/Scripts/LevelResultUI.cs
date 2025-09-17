using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LevelResultUI : MonoBehaviour
{
    public static LevelResultUI Instance;

    [Header("UI")]
    public CanvasGroup overlay;          // CanvasGroup על ResultOverlay (הרקע הכהה)
    public RectTransform window;         // הפאנל המרכזי
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    [Header("Stars (סדר משמאל לימין)")]
    public Image star1, star2, star3;

    [Header("Buttons")]
    public Button retryBtn, homeBtn, nextBtn;

    [Header("Scenes")]
    public string levelSelectScene = "LevelSelect";
    public string mainScene        = "Main";

    [Header("Animation")]
    public float fadeDuration   = 0.25f;
    public float scaleDuration  = 0.30f;
    public float starPopDelay   = 0.08f;
    public Ease  windowEase     = Ease.OutBack;

    [Header("Title Colors")]
    public Color winTitleColor  = new Color(0.12f, 0.65f, 0.40f, 1f); // ירקרק
    public Color loseTitleColor = new Color(0.82f, 0.26f, 0.26f, 1f); // אדמדם

    [Header("Stars Colors")]
    public Color starLitColor = new Color(1f, 0.84f, 0.35f, 1f);      // זהב
    public Color starOffColor = new Color(0.78f, 0.81f, 0.86f, 1f);   // אפור בהיר

    Image[] _stars;
    Sequence _seq;
    int _currentLevel;

    void Awake()
    {
        Instance = this;
        _stars = new[] { star1, star2, star3 };
        gameObject.SetActive(false);
    }

    void Start()
    {
        _currentLevel = PlayerPrefs.GetInt("current_level", 1);
        if (retryBtn) retryBtn.onClick.AddListener(OnRetry);
        if (homeBtn)  homeBtn.onClick.AddListener(OnHome);
        if (nextBtn)  nextBtn.onClick.AddListener(OnNext);
    }

    void OnDisable()
    {
        _seq?.Kill();
    }

    // ---------- API שהמשחק קורא ----------
    public void ShowWin(int score, int stars)
    {
        SaveProgress(_currentLevel, stars, unlockNext: true);
        Open(
            title: "LEVEL COMPLETE!",
            titleColor: winTitleColor,
            score: score,
            showNext: true,
            stars: Mathf.Clamp(stars, 0, 3));
    }

    public void ShowLose(int score)
    {
        SaveProgress(_currentLevel, 0, unlockNext: false);
        Open(
            title: "LEVEL FAILED",
            titleColor: loseTitleColor,
            score: score,
            showNext: false,     // <<<< אין Next בהפסד
            stars: 0             // <<<< כוכבים כבויים
        );
    }

    // ---------- לוגיקת הצגה ואנימציה ----------
    void Open(string title,Color titleColor, int score, bool showNext, int stars)
    {
        gameObject.SetActive(true);

        // טקסטים וכפתורים
        titleText.enableWordWrapping = false;            
        titleText.overflowMode       = TextOverflowModes.Overflow;
        titleText.color = titleColor;
        titleText.text  = title;

        scoreText.enableWordWrapping = false;
        scoreText.overflowMode       = TextOverflowModes.Overflow;
        scoreText.text = $"Score: {score:n0}";

        // כפתורים
        nextBtn.gameObject.SetActive(showNext);
        retryBtn.gameObject.SetActive(true);
        homeBtn.gameObject.SetActive(true);

        // הכנה: לכבות/לאפס כוכבים
        for (int i = 0; i < _stars.Length; i++)
        {
            var img = _stars[i];
            if (!img) continue;
            bool lit = i < stars;
            img.color = lit ? starLitColor : starOffColor;
            img.transform.localScale = lit ? Vector3.one * 0.01f : Vector3.one;
        }

        // הכנה לאוברליי/חלון
        overlay.alpha = 0f;
        overlay.interactable = true;
        overlay.blocksRaycasts = true;
        window.localScale = Vector3.one * 0.85f;

        // הורג סיקוונס קודם אם קיים
        _seq?.Kill();

        // בונים רצף אנימציה
        _seq = DOTween.Sequence().SetUpdate(true); // לרוץ גם אם Time.timeScale=0 (פאוז)

        // Fade לרקע + פופ לחלון
        _seq.Append(overlay.DOFade(1f, fadeDuration));
        _seq.Join(window.DOScale(1f, scaleDuration).SetEase(windowEase));

        // פופ לכוכבים דלוקים (רק אלו שסטארס מאפשר)
        for (int i = 0; i < stars && i < _stars.Length; i++)
        {
            var t = _stars[i]?.transform;
            if (!t) continue;
            _seq.AppendInterval(starPopDelay);
            _seq.Append(t.DOScale(1f, 0.22f).SetEase(Ease.OutBack));
            _seq.Append(t.DOPunchScale(Vector3.one * 0.06f, 0.12f, 1, 0.8f));
        }
    }

    // ---------- ניווט ----------
    void OnRetry()
    {
        SceneManager.LoadScene(mainScene);
    }

    void OnHome()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    void OnNext()
    {
        int next = _currentLevel + 1;
        PlayerPrefs.SetInt("current_level", next);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainScene);
    }

    // ---------- שמירת התקדמות ----------
    void SaveProgress(int levelIndex, int stars, bool unlockNext)
    {
        string k = $"level_stars_{levelIndex}";
        int prev = PlayerPrefs.GetInt(k, 0);
        if (stars > prev) PlayerPrefs.SetInt(k, stars);
        if (unlockNext)   PlayerPrefs.SetInt($"level_unlocked_{levelIndex + 1}", 1);
        PlayerPrefs.Save();
    }
}
