using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public Grid board;
    public ResourceManagement bank;
    public enum PieceType { WATER, SUN, EARTH, GRASS };
    public Button btnBlue, btnRed, btnPink, btnGreen;

    public GameObject infoPanel;
    public TMP_Text titleText, costText, effectText;
    public Button buyButton, closeButton;
    public bool showAsModal = false;
    //public GameObject overlay; 
    Transform infoOriginalParent;   // to restore parent on close
    public class FlowerDef
    {
        public string title;
        public Cost price;
        public Grid.TargetMode mode;
        public string description;
    }
    public FlowerDef current;
    public FlowerDef Blue() => new FlowerDef
    {
        title = "Blue Flower",
        price = new Cost { water = 10 },
        mode = Grid.TargetMode.Row,
        description = "Clears a row"
    };
    public FlowerDef Red() => new FlowerDef
    {
        title = "Red Flower",
        price = new Cost { earth = 3, water = 1, sun = 1 },
        mode = Grid.TargetMode.Column,
        description = "Clears a column"
    };
    public FlowerDef Pink() => new FlowerDef
    {
        title = "Pink Flower",
        price = new Cost { water = 3, sun = 3, grass = 3, earth = 3 },
        mode = Grid.TargetMode.AllOfType,
        description = "Clears all of one type"
    };
    public FlowerDef Green() => new FlowerDef
    {
        title = "Green Flower",
        price = new Cost { grass = 2, water = 2, sun = 2 },
        mode = Grid.TargetMode.Cell3x3,
        description = "Clears a 3x3 area"
    };


    void Awake()
    {
        infoOriginalParent = infoPanel.transform.parent;

        if (btnBlue != null) btnBlue.onClick.AddListener(() => ShowInfo(Blue(), (RectTransform)btnBlue.transform));
        if (btnRed != null) btnRed.onClick.AddListener(() => ShowInfo(Red(), (RectTransform)btnRed.transform));
        if (btnPink != null) btnPink.onClick.AddListener(() => ShowInfo(Pink(), (RectTransform)btnPink.transform));
        if (btnGreen != null) btnGreen.onClick.AddListener(() => ShowInfo(Green(), (RectTransform)btnGreen.transform));
        
        if (closeButton) closeButton.onClick.AddListener(CloseInfo);
        if (infoPanel) infoPanel.SetActive(false);
        //if (overlay) overlay.SetActive(false);
        if (bank) bank.OnChanged += RefreshAllItems;
    }
    void OnDestroy()
    {
        if (bank) bank.OnChanged -= RefreshAllItems;
    }
    public void ShowInfo(FlowerDef def, RectTransform anchor = null,int qty = 1)
    {
        if (!infoPanel) return;

        titleText.text = def.title;
        costText.text = FormatCost(def.price, qty);
        effectText.text = def.description;


        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => TryBuy(def));
            buyButton.interactable = bank ? bank.Has(def.price) : false;
        }
        var panelRT = (RectTransform)infoPanel.transform;
        panelRT.SetParent(infoOriginalParent, false);
        if (!showAsModal && anchor != null)
        {
            Vector2 localPoint;
            RectTransform parentRT = (RectTransform)infoOriginalParent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRT,
            RectTransformUtility.WorldToScreenPoint(null, anchor.position),
            null,
            out localPoint);
            panelRT.pivot = new Vector2(0.5f, 0f);     // תחתית הפאנל תהיה ליד העוגן
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = localPoint + new Vector2(0f, 120f);
        }
        else
        {
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = Vector2.zero;
        }
        infoPanel.SetActive(true);
    }
    public void CloseInfo()
    {
        var panelRT = (RectTransform)infoPanel.transform;
        panelRT.SetParent(infoOriginalParent, false);
        infoPanel.SetActive(false);
        //overlay.SetActive(false);
    }

    public void TryBuy(FlowerDef def,int qty = 1)
    {
        if (def == null || bank == null || board == null) return;
        var total = Cost.Multiply(def.price, qty);

        if (!bank.Spend(total))
        {
            Debug.Log("Not enough resources");
            return;
        }
        board.EnterTargetMode(def.mode);
        infoPanel.SetActive(false);
        CloseInfo();
    }

    public enum FlowerKind { Blue, Red, Pink, Green }
    public string FormatCost(Cost c) {
    string s = "";
    if (c.water > 0) s += $"{c.water} ";
    if (c.sun   > 0) s += $"{c.sun} ";
    if (c.earth > 0) s += $"{c.earth} ";
    if (c.grass > 0) s += $"{c.grass} ";
    return string.IsNullOrEmpty(s) ? "Free" : s.Trim();
}

    public string FormatCost(Cost c, int qty = 1)
    {
        var t = Cost.Multiply(c, qty);
        string s = "";
        if (c.water > 0) s += $"{c.water}  ";
        if (c.sun > 0) s += $"{c.sun} ";
        if (c.earth > 0) s += $"{c.earth}  ";
        if (c.grass > 0) s += $"{c.grass}  ";
        return string.IsNullOrEmpty(s) ? "Free" : s.Trim();
    }

    public FlowerDef GetDef(FlowerKind k)
    {
        switch (k)
        {
            case FlowerKind.Blue: return Blue();
            case FlowerKind.Red: return Red();
            case FlowerKind.Pink: return Pink();
            case FlowerKind.Green: return Green();
            default: return Blue();

        }
    }
     public void RefreshAllItems()
    {
        var items = GetComponentsInChildren<StoreItem>(true);
        foreach (var it in items) it.RefreshState();
    }
    

    
}
