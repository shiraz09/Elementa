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
        // מאזינים
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
        ApplyMusicState();
    }

    // -------- Buttons --------
    void OnPlay()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    void ToggleInfo()
    {
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
        musicOn = !musicOn;
        ApplyMusicState();
        PlayerPrefs.SetInt(MUSIC_PREF, musicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyMusicState()
    {
        if (musicSource)
        {
            musicSource.mute = !musicOn;
            if (musicOn && !musicSource.isPlaying) musicSource.Play();
        }
        else
        {
            // פולבאק – השתקה גלובלית אם אין מקור מוגדר
            AudioListener.pause = !musicOn;
        }

        if (soundBtnImg)
            soundBtnImg.sprite = musicOn ? soundOnSprite : soundOffSprite;
    }
}