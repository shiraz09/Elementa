using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class HomeUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button infoButton;
    public Button soundButton;

    [Header("Info Panel")]
    public GameObject infoPanelRoot;     // ה-Panel הגדול (אפשר עם רקע כהה)
    public CanvasGroup infoCanvasGroup;  // על אותו Root (כדי לעשות Fade)
    public Button infoCloseButton;       // כפתור X לסגירה (אופציונלי)

    [Header("Sound")]
    public AudioSource musicSource;      // ה-AudioSource של המוזיקה
    public Sprite soundOnSprite;         // אייקון ON
    public Sprite soundOffSprite;        // אייקון OFF

    [Header("Navigation")]
    public string gameSceneName = "OpeningScreen";// שם הסצנה של המשחק

    const string MUSIC_PREF = "music_on";
    bool musicOn = true;
    Image soundBtnImg;

    void Awake()
    {
        if (soundButton) soundBtnImg = soundButton.GetComponent<Image>();
    }

    void Start()
    {
        // מאזינים לכפתורים
        if (playButton)     playButton.onClick.AddListener(OnPlay);
        if (infoButton)     infoButton.onClick.AddListener(ToggleInfo);
        if (infoCloseButton)infoCloseButton.onClick.AddListener(ToggleInfo);
        if (soundButton)    soundButton.onClick.AddListener(ToggleMusic);

        // מצב התחלתי של פאנל מידע
        if (infoPanelRoot) infoPanelRoot.SetActive(false);
        if (infoCanvasGroup)
        {
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }

        // מוזיקה: טען העדפה ושקף לאודיו/אייקון
        musicOn = PlayerPrefs.GetInt(MUSIC_PREF, 1) == 1;
        
        // החלת ההגדרות על האודיו
        ApplyMusicState();
        
        // הפעלת מוזיקת רקע (רק אם הסאונד מופעל)
        if (AudioManager.Instance != null && musicOn)
            AudioManager.Instance.PlayMusic(0);
    }

    // -------- Buttons --------
    void OnPlay()
    {
        // השמעת צליל לחיצה
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("click");
            
        // מעבר לסצנת המשחק אחרי השהייה קצרה
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // אפשרות 1: מעבר מיידי
            // SceneManager.LoadScene(gameSceneName);
            
            // אפשרות 2: מעבר אחרי השהייה קצרה כדי לאפשר לצליל להישמע
            Invoke("LoadGameScene", 0.2f);
        }
    }
    
    void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    void ToggleInfo()
    {
        // השמעת צליל לחיצה
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("click");
            
        if (!infoPanelRoot) return;

        bool open = !infoPanelRoot.activeSelf;
        infoPanelRoot.SetActive(true); // להבטיח שהוא פעיל לפני Fade-In/Out

        if (infoCanvasGroup) // Fade נחמד עם DOTween (אם מותקן)
        {
            infoCanvasGroup.DOKill();
            if (open)
            {
                infoCanvasGroup.interactable = true;
                infoCanvasGroup.blocksRaycasts = true;
                infoCanvasGroup.DOFade(1f, 0.25f);
            }
            else
            {
                infoCanvasGroup.interactable = false;
                infoCanvasGroup.blocksRaycasts = false;
                infoCanvasGroup.DOFade(0f, 0.2f)
                    .OnComplete(() => infoPanelRoot.SetActive(false));
            }
        }
        else
        {
            infoPanelRoot.SetActive(open);
        }
    }

    void ToggleMusic()
    {
        // השמעת צליל לחיצה (רק אם הסאונד מופעל)
        if (AudioManager.Instance != null && musicOn)
            AudioManager.Instance.PlaySound("click");
            
        // החלפת מצב המוזיקה
        musicOn = !musicOn;
        
        // החלת השינויים
        ApplyMusicState();
    }

    void ApplyMusicState()
    {
        // השתקת המוזיקה המקומית (אם מוגדרת)
        if (musicSource)
        {
            musicSource.mute = !musicOn;
            if (musicOn && !musicSource.isPlaying) musicSource.Play();
        }
        
        // השתקת/הפעלת AudioManager (מוזיקה + אפקטים)
        if (AudioManager.Instance != null)
        {
            // השתקת/הפעלת מוזיקת רקע
            AudioManager.Instance.ToggleMusic(musicOn);
            
            // השתקת/הפעלת אפקטי סאונד
            AudioManager.Instance.ToggleSFX(musicOn);
            
            // עדכון עוצמת הקול הכללית
            AudioManager.Instance.masterVolume = musicOn ? 1f : 0f;
            AudioManager.Instance.UpdateVolume();
        }
        else
        {
            // פולבאק – השתקה גלובלית אם אין AudioManager
            AudioListener.pause = !musicOn;
        }

        // החלפת האייקון של הכפתור
        if (soundBtnImg)
            soundBtnImg.sprite = musicOn ? soundOnSprite : soundOffSprite;
            
        // שמירת ההעדפה
        PlayerPrefs.SetInt(MUSIC_PREF, musicOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}