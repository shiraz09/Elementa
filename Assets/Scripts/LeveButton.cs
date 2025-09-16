using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]   // מעדכן גם כשלא במצב Play
public class LevelButton : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private int stageIndex = 1;
    public int StageIndex
    {
        get => stageIndex;
        set { stageIndex = Mathf.Max(1, value); ApplyIndexToLabel(); }
    }

    [Header("UI")]
    public Button button;
    public TMP_Text label;       // גררי את הילד LevelNum
    public Image starsImage;
    public Sprite[] starStates;

    [Header("Lock visuals (optional)")]
    public GameObject lockOverlay;

    private void OnEnable()
    {
        AutoBind();
        ApplyIndexToLabel();   // מציב טקסט גם באדיטור
        if (Application.isPlaying)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
            RefreshRuntime();
        }
    }

    private void Update()
    {
        // גם באדיטור: אם StageIndex השתנה או שכפלת כפתור – נעדכן טקסט
        if (!Application.isPlaying)
            ApplyIndexToLabel();
    }

    private void AutoBind()
    {
        if (label == null)
        {
            var t = transform.Find("LevelNum");
            if (t) label = t.GetComponent<TMP_Text>();
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        }
        if (button == null) button = GetComponent<Button>();
        if (starsImage == null) starsImage = GetComponentInChildren<Image>(true);
    }

    private void ApplyIndexToLabel()
    {
        if (label != null)
            label.text = Mathf.Max(1, stageIndex).ToString();
    }

    /// <summary>
    /// בזמן ריצה – רענון כוכבים ונעילה.
    /// </summary>
    public void Refresh()
    {
        ApplyIndexToLabel();  // נשמור טקסט תמיד
        if (Application.isPlaying)
            RefreshRuntime();
    }

    private void RefreshRuntime()
    {
        // מצב כוכבים
        if (starsImage != null && starStates != null && starStates.Length >= 4)
        {
            int stars = ProgressManager.GetStars(stageIndex);
            starsImage.sprite = starStates[Mathf.Clamp(stars, 0, 3)];
        }

        // מצב נעילה
        bool unlocked = ProgressManager.IsUnlocked(stageIndex);
        if (button != null) button.interactable = unlocked;
        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
    }

    private void OnClick()
    {
        if (!ProgressManager.IsUnlocked(stageIndex)) return;
        LevelLoader.LoadStage(stageIndex, "Main");
    }
}