using UnityEngine;
using UnityEngine.UI;

public class RefreshButton : MonoBehaviour
{
    [Header("UI References")]
    public Button refreshButton;
    public Grid grid;
    
    [Header("Visual Settings")]
    public float cooldownTime = 5f; // זמן המתנה בין רענונים
    public Image cooldownImage; // תמונה להצגת זמן המתנה
    
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    
    void Start()
    {
        // מציאת Grid אם לא הוגדר
        if (grid == null)
            grid = FindFirstObjectByType<Grid>();
        
        // הגדרת כפתור
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }
        
        // אתחול תמונת זמן המתנה
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
            cooldownImage.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // עדכון זמן המתנה
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = cooldownTimer / cooldownTime;
            }
            
            if (cooldownTimer <= 0f)
            {
                EndCooldown();
            }
        }
    }
    
    public void OnRefreshClicked()
    {
        if (isOnCooldown || grid == null) return;
        
        Debug.Log("🔄 Manual board refresh requested!");
        
        // רענון הלוח
        grid.ManualRefreshBoard();
        
        // התחלת זמן המתנה
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        
        // השבתת הכפתור
        if (refreshButton != null)
        {
            refreshButton.interactable = false;
        }
        
        // הצגת תמונת זמן המתנה
        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(true);
        }
    }
    
    private void EndCooldown()
    {
        isOnCooldown = false;
        
        // הפעלת הכפתור
        if (refreshButton != null)
        {
            refreshButton.interactable = true;
        }
        
        // הסתרת תמונת זמן המתנה
        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(false);
        }
    }
    
    // פונקציה להגדרת זמן המתנה
    public void SetCooldownTime(float newCooldownTime)
    {
        cooldownTime = newCooldownTime;
    }
    
    // פונקציה לבדיקה אם הכפתור זמין
    public bool IsButtonAvailable()
    {
        return !isOnCooldown;
    }
    
    // פונקציה לקבלת זמן המתנה נותר
    public float GetRemainingCooldown()
    {
        return isOnCooldown ? cooldownTimer : 0f;
    }
}
