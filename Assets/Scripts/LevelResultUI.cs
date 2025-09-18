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
    public Image backgroundPanel;        // רקע הפופאפ

    [Header("Stars (סדר משמאל לימין)")]
    public Image star1, star2, star3;
    public RectTransform starsContainer; // מיכל לכוכבים (לאנימציה)

    [Header("Buttons")]
    public Button retryBtn, homeBtn, nextBtn;
    public Image retryBtnImage;          // תמונת כפתור retry
    public Image homeBtnImage;           // תמונת כפתור home
    public Image nextBtnImage;           // תמונת כפתור next

    [Header("Scenes")]
    public string levelSelectScene = "LevelSelect";
    public string mainScene        = "Main";

    [Header("Animation")]
    public float fadeDuration   = 0.25f;
    public float scaleDuration  = 0.30f;
    public float starPopDelay   = 0.08f;
    public Ease  windowEase     = Ease.OutBack;
    public Ease  starEase       = Ease.OutElastic;

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
        
        // וידוא שהרפרנסים לתמונות הכפתורים מוגדרים
        if (retryBtn && retryBtnImage == null) 
            retryBtnImage = retryBtn.GetComponent<Image>();
            
        if (homeBtn && homeBtnImage == null) 
            homeBtnImage = homeBtn.GetComponent<Image>();
            
        if (nextBtn && nextBtnImage == null) 
            nextBtnImage = nextBtn.GetComponent<Image>();
    }

    void Start()
    {
        _currentLevel = PlayerPrefs.GetInt("current_level", 1);
        
        // הגדרת מאזינים לכפתורים
         if (retryBtn) {
            retryBtn.onClick.RemoveAllListeners(); // נקה מאזינים קודמים
            retryBtn.onClick.AddListener(OnRetry);
            Debug.Log("Retry button listener added");
        }
        
        if (homeBtn) {
            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(OnHome);
            Debug.Log("Home button listener added");
        }
        
        if (nextBtn) {
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(OnNext);
            Debug.Log("Next button listener added");
        }
            
        // הגדרת כפתורים יפים
        SetupButtonVisuals(retryBtn, retryBtnImage);
        SetupButtonVisuals(homeBtn, homeBtnImage);
        SetupButtonVisuals(nextBtn, nextBtnImage);
    }

    private void ResetInteractivity()
    {
        // וודא שהאינטראקציה פעילה
        if (overlay)
        {
            overlay.interactable = true;
            overlay.blocksRaycasts = true;
        }
        
        // וודא שכל הכפתורים פעילים
        if (retryBtn) retryBtn.interactable = true;
        if (homeBtn) homeBtn.interactable = true;
        if (nextBtn) nextBtn.interactable = true;
    }
        
    // עיצוב הכפתורים
    void SetupButtonVisuals(Button btn, Image img)
    {
        if (btn == null || img == null) return;
        
        // הגדרת שמירת יחס תמונה
        img.preserveAspect = true;
        
        // הוספת אפקט מעבר עכבר אם אין כבר
        var btnColors = btn.colors;
        btnColors.normalColor = Color.white;
        btnColors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        btnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btnColors.selectedColor = new Color(1f, 1f, 1f, 1f);
        btnColors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        btnColors.colorMultiplier = 1f;
        btnColors.fadeDuration = 0.1f;
        btn.colors = btnColors;
    }

    void OnDisable()
    {
        _seq?.Kill();
    }

    // ---------- API שהמשחק קורא ----------
    public void ShowWin(int score, int stars)
    {
        // עצירת זמן המשחק
        Time.timeScale = 0f;
        
        // שמירת התקדמות
        SaveProgress(_currentLevel, stars, unlockNext: true);
        
        // הצגת חלון עם הודעת ניצחון מותאמת
        string winTitle = GetWinTitle(stars);
        
        Open(
            title: winTitle,
            titleColor: winTitleColor,
            score: score,
            showNext: true,
            stars: Mathf.Clamp(stars, 0, 3));
    }

    public void ShowLose(int score)
    {
        // עצירת זמן המשחק
        Time.timeScale = 0f;
        
        // שמירת התקדמות
        SaveProgress(_currentLevel, 0, unlockNext: false);
        
        // הצגת חלון עם הודעת הפסד מותאמת
        string loseTitle = GetLoseTitle();
        
        Open(
            title: loseTitle,
            titleColor: loseTitleColor,
            score: score,
            showNext: false,     // <<<< אין Next בהפסד
            stars: 0             // <<<< כוכבים כבויים
        );
        
    }
    
    // הודעות ניצחון מגוונות לפי מספר הכוכבים
    private string GetWinTitle(int stars)
    {
        switch (stars)
        {
            case 3: return "Perfect!";
            case 2: return "Well done";
            case 1: return "You succeeded.!";
            default: return "You have completed the step.!";
        }
    }
    
    // הודעות הפסד מגוונות
    private string GetLoseTitle()
    {
        string[] messages = {
            "Try again!",
            "You nearly did it..",
            "Don't give up"
        };
        
        return messages[Random.Range(0, messages.Length)];
    }

    // ---------- לוגיקת הצגה ואנימציה ----------
    void Open(string title, Color titleColor, int score, bool showNext, int stars)
    {
        gameObject.SetActive(true);
         // הוסף סאונד פתיחת פופאפ
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("popup_open");
        ResetInteractivity();

        // עיצוב הרקע של הפופאפ
        if (backgroundPanel != null)
        {
            backgroundPanel.color = new Color(1f, 1f, 1f, 1f);
        }

        // טקסטים וכפתורים - עיצוב יפה יותר
        titleText.enableWordWrapping = false;            
        titleText.overflowMode = TextOverflowModes.Overflow;
        titleText.color = titleColor;
        titleText.text = title;
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyles.Bold;

        scoreText.enableWordWrapping = false;
        scoreText.overflowMode = TextOverflowModes.Overflow;
        scoreText.text = $"Score: {score:n0}";
        scoreText.fontSize = 28;
        
        // כפתורים - עיצוב והפעלה
        nextBtn.gameObject.SetActive(showNext);
        retryBtn.gameObject.SetActive(true);
        homeBtn.gameObject.SetActive(true);
        
        // עדכון תמונות לכפתורים אם הוגדרו
        if (retryBtnImage != null) retryBtnImage.preserveAspect = true;
        if (homeBtnImage != null) homeBtnImage.preserveAspect = true;
        if (nextBtnImage != null) nextBtnImage.preserveAspect = true;

        // הכנה: לכבות/לאפס כוכבים
        for (int i = 0; i < _stars.Length; i++)
        {
            var img = _stars[i];
            if (!img) continue;
            bool lit = i < stars;
            img.color = lit ? starLitColor : starOffColor;
            img.transform.localScale = lit ? Vector3.one * 0.01f : Vector3.one;
            img.preserveAspect = true;
        }

        // הכנה לאוברליי/חלון
        overlay.alpha = 0f;
        overlay.interactable = true;
        overlay.blocksRaycasts = true;
        window.localScale = Vector3.one * 0.85f;

        // הורג סיקוונס קודם אם קיים
        _seq?.Kill();

        // בונים רצף אנימציה משופר
        _seq = DOTween.Sequence().SetUpdate(true); // לרוץ גם אם Time.timeScale=0 (פאוז)

        // Fade לרקע + פופ לחלון עם אפקט יפה
        _seq.Append(overlay.DOFade(1f, fadeDuration));
        _seq.Join(window.DOScale(1f, scaleDuration).SetEase(windowEase));
        
        // אנימציית סיבוב קלה לחלון
        window.rotation = Quaternion.identity;
        // אנימציה למיכל הכוכבים אם קיים
        if (starsContainer != null)
        {
            _seq.Append(starsContainer.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad));
            _seq.Append(starsContainer.DOScale(1f, 0.2f).SetEase(Ease.OutBounce));
        }

        // פופ לכוכבים דלוקים עם אנימציה משופרת
        for (int i = 0; i < stars && i < _stars.Length; i++)
        {
            var t = _stars[i]?.transform;
            if (!t) continue;
            _seq.AppendInterval(starPopDelay);
            _seq.Append(t.DOScale(1.2f, 0.3f).SetEase(starEase));
             if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("star_appear");
            _seq.Join(t.DORotate(new Vector3(0, 0, 20), 0.3f, RotateMode.Fast).SetEase(Ease.OutQuad));
            _seq.Append(t.DOScale(1f, 0.15f).SetEase(Ease.OutBounce));
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("star_appear");
            _seq.Join(t.DORotate(Vector3.zero, 0.15f, RotateMode.Fast).SetEase(Ease.OutQuad));
            _seq.Append(t.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.8f));
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("star_appear");
        }
        
        // אנימציה לכפתורים
        if (showNext && nextBtn != null)
        {
            nextBtn.transform.localScale = Vector3.zero;
            _seq.Append(nextBtn.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad));
            _seq.Append(nextBtn.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBounce));
        }
        
        retryBtn.transform.localScale = Vector3.zero;
        homeBtn.transform.localScale = Vector3.zero;
        
        _seq.Append(retryBtn.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad));
        _seq.Join(homeBtn.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad).SetDelay(0.1f));
        
        _seq.Append(retryBtn.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBounce));
        _seq.Join(homeBtn.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBounce));

        _seq.OnComplete(() => {

            overlay.interactable = true;
            overlay.blocksRaycasts = true;
            
            if (retryBtn) retryBtn.transform.localScale = Vector3.one;
            if (homeBtn) homeBtn.transform.localScale = Vector3.one;
            if (nextBtn && showNext) nextBtn.transform.localScale = Vector3.one;
        });
    }

    // ---------- ניווט עם אנימציית לחיצה ----------
    void OnRetry()
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySound("click");
        
        PlayButtonAnimation(retryBtn.transform, () => {
            SceneManager.LoadScene(mainScene);
        });
    }

    void OnHome()
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySound("click");

        PlayButtonAnimation(homeBtn.transform, () => {
            SceneManager.LoadScene(levelSelectScene);
        });
    }

    void OnNext()
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySound("click");

        PlayButtonAnimation(nextBtn.transform, () => {
            int next = _currentLevel + 1;
            PlayerPrefs.SetInt("current_level", next);
            PlayerPrefs.Save();
            SceneManager.LoadScene(mainScene);
        });
    }
    
    // אנימציית לחיצת כפתור
    void PlayButtonAnimation(Transform buttonTransform, System.Action onComplete)
    {
        
        
        // אנימציית לחיצה
        buttonTransform.DOScale(0.85f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
            buttonTransform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
                buttonTransform.DOScale(1f, 0.1f).SetEase(Ease.OutBounce).OnComplete(() => {
                    overlay.interactable = true;
                    // המתנה קצרה לפני מעבר
                    DOVirtual.DelayedCall(0.1f, () => {
                        onComplete?.Invoke();
                    });
                });
            });
        });
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
