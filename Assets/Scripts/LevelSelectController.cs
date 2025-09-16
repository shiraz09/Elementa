using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelSelectController
/// - מנהל עמודים של כפתורי שלבים (כל עמוד מכיל כמה LevelButton-ים).
/// - מציג עמוד אחד בכל רגע, עם חיצי ניווט שמאל/ימין.
/// - מרענן את העמוד הפעיל בזמן ריצה כדי למשוך כוכבים/סטטוס נעילה.
/// - כולל כלים לאדיטור: מספור אוטומטי ורענון כל העמודים.
/// </summary>
[DisallowMultipleComponent]
public class LevelSelectController : MonoBehaviour
{
    [Header("Pages (each GameObject holds a grid of LevelButton children)")]
    [Tooltip("סדרי כאן את כל העמודים. רק עמוד אחד יוצג בכל רגע.")]
    public List<GameObject> pages = new List<GameObject>();

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Start Page")]
    [Min(0)] public int startPageIndex = 0;

    [Header("Behavior")]
    [Tooltip("לבצע Refresh לעמוד הנוכחי בעת העלאה (Play/Enable).")]
    public bool refreshOnEnable = true;

    [Tooltip("לבצע Refresh לעמוד הנוכחי אחרי כל ניווט עמוד.")]
    public bool refreshAfterNavigation = true;

    private int pageIndex = 0;

    // --------- Unity lifecycle ---------
    private void Awake()
    {
        ClampAndSetStartPage();
        UpdatePageVisibility();
        WireButtons();
        if (refreshOnEnable && Application.isPlaying)
            RefreshCurrentPage();
    }

    private void OnEnable()
    {
        // במידה והאובייקט הושבת/הופעל מחדש
        UpdatePageVisibility();
        if (refreshOnEnable && Application.isPlaying)
            RefreshCurrentPage();
    }

    // --------- Public API ---------
    public void NextPage()
    {
        if (pages == null || pages.Count == 0) return;
        if (pageIndex < pages.Count - 1)
        {
            pageIndex++;
            UpdatePageVisibility();
            if (refreshAfterNavigation && Application.isPlaying)
                RefreshCurrentPage();
        }
    }

    public void PrevPage()
    {
        if (pages == null || pages.Count == 0) return;
        if (pageIndex > 0)
        {
            pageIndex--;
            UpdatePageVisibility();
            if (refreshAfterNavigation && Application.isPlaying)
                RefreshCurrentPage();
        }
    }

    /// <summary>
    /// מעבר לעמוד מסוים לפי אינדקס (0-based).
    /// </summary>
    public void GoToPage(int index)
    {
        if (pages == null || pages.Count == 0) return;
        int clamped = Mathf.Clamp(index, 0, pages.Count - 1);
        if (clamped == pageIndex) return;

        pageIndex = clamped;
        UpdatePageVisibility();
        if (refreshAfterNavigation && Application.isPlaying)
            RefreshCurrentPage();
    }

    /// <summary>
    /// כמה עמודים יש בתצוגה?
    /// </summary>
    public int PageCount => pages != null ? pages.Count : 0;

    /// <summary>
    /// מה האינדקס של העמוד הפעיל (0-based)?
    /// </summary>
    public int CurrentPageIndex => pageIndex;

    /// <summary>
    /// רענון כל ה-LevelButton בעמוד הנוכחי (כוכבים/נעילה/טקסט לפי המימוש של LevelButton.Refresh()).
    /// </summary>
    public void RefreshCurrentPage()
    {
        if (pages == null || pages.Count == 0) return;
        var page = pages[Mathf.Clamp(pageIndex, 0, pages.Count - 1)];
        if (page == null) return;

        var buttons = page.GetComponentsInChildren<LevelButton>(true);
        foreach (var b in buttons)
        {
            if (b != null)
                b.Refresh();
        }
    }

    /// <summary>
    /// רענון כל העמודים (עלול להיות יקר אם יש הרבה כפתורים).
    /// </summary>
    public void RefreshAllPages()
    {
        if (pages == null) return;
        foreach (var page in pages)
        {
            if (page == null) continue;
            var buttons = page.GetComponentsInChildren<LevelButton>(true);
            foreach (var b in buttons)
            {
                if (b != null)
                    b.Refresh();
            }
        }
    }

    // --------- Internal helpers ---------
    private void WireButtons()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(PrevPage);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(NextPage);
        }

        UpdateNavInteractable();
    }

    private void ClampAndSetStartPage()
    {
        int count = pages != null ? pages.Count : 0;
        pageIndex = (count == 0) ? 0 : Mathf.Clamp(startPageIndex, 0, count - 1);
    }

    private void UpdatePageVisibility()
    {
        int count = pages != null ? pages.Count : 0;

        for (int i = 0; i < count; i++)
        {
            var go = pages[i];
            if (go != null)
                go.SetActive(i == pageIndex);
        }

        UpdateNavInteractable();
    }

    private void UpdateNavInteractable()
    {
        int count = pages != null ? pages.Count : 0;

        if (leftButton != null)
            leftButton.interactable = (count > 0 && pageIndex > 0);

        if (rightButton != null)
            rightButton.interactable = (count > 0 && pageIndex < count - 1);
    }

    // --------- Editor Utilities ---------
#if UNITY_EDITOR
    [ContextMenu("Auto Number LevelButtons (Editor)")]
    private void AutoNumberLevelButtons_Context()
    {
        AutoNumberLevelButtons();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("LevelButton indices updated.");
    }

    /// <summary>
    /// ממספר אוטומטית את כל ה-LevelButton לפי סדר העמודים וההיררכיה.
    /// שיטי להשתמש מהקונטקסט-מניו או להריץ ידנית.
    /// </summary>
    public void AutoNumberLevelButtons()
    {
        int idx = 1;
        foreach (var page in pages)
        {
            if (page == null) continue;
            var buttons = page.GetComponentsInChildren<LevelButton>(true);

            // אפשר למיין לפי סדר ב-Hierarchy אם רוצים עקביות:
            // System.Array.Sort(buttons, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            foreach (var b in buttons)
            {
                if (b == null) continue;
                b.StageIndex = idx++;
                UnityEditor.EditorUtility.SetDirty(b);
            }
        }
    }

    [ContextMenu("Refresh All Pages (Editor)")]
    private void ContextRefreshAll()
    {
        RefreshAllPages();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif
}