using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public Grid board;
    public ResourceManagement bank;
    public enum PieceType { WATER, SUN, EARTH, GRASS };
    public Button btnBlue, btnRed, btnPink, btnGreen;
    public BoardAbility WaterAbility;
    public BoardAbility EarthAbility;
    public BoardAbility SunAbility;
    public BoardAbility GrassAbility;
    BoardAbility current;
    
    public bool showAsModal = false;
    //public GameObject overlay; 
    Transform infoOriginalParent;   // to restore parent on close
    public class FlowerDef
    {
        public string title;
        public Cost price;
        public BoardAbility ability;
        public string description;
    }
    public FlowerDef WaterFlower() => new FlowerDef
    {
        title = "Water Flower",
        price = new Cost { water = 20 },
        ability=WaterAbility,
        description = "Clears a row"
    };
    public FlowerDef EarthFlower() => new FlowerDef
    {
        title = "Earth Flower",
        price = new Cost { earth = 16, water = 4, sun = 8 },
        ability=EarthAbility,
        description = "Clears a column"
    };
    public FlowerDef SunFlower() => new FlowerDef
    {
        title = "Sun Flower",
        price = new Cost { water = 3, sun = 25, grass = 7, earth = 3 },
        ability=SunAbility,
        description = "Clears all of one type"
    };
    public FlowerDef GrassFlower() => new FlowerDef
    {
        title = "Grass Flower",
        price = new Cost { grass = 20, water = 10, sun = 10, earth = 15 },
        ability=GrassAbility,
        description = "Clears a 3x3 area"
    };


    void Awake()
    {
        //if (overlay) overlay.SetActive(false);
        if (bank) bank.OnChanged += RefreshAllItems;
        RefreshAllItems();
    }
    void OnEnable()
    {
        if (bank) bank.OnChanged += RefreshAllItems;
        RefreshAllItems();
    }
    void OnDestroy()
    {
        if (bank) bank.OnChanged -= RefreshAllItems;
    }

    public void TryBuy(BoardAbility ab, int qty = 1)
    {
        if (!ab || !bank || !board) return;
        var total = Cost.Multiply(ab.price, qty);
        if (!bank.Spend(total)) { Debug.Log("Not enough resources"); return; }
        board.EnterAbility(ab);
    }

    public enum FlowerKind { WaterFlower, EarthFlower, SunFlower, GrassFlower }
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
        if (t.water > 0) s += $"{t.water}  ";
        if (t.sun > 0) s += $"{t.sun} ";
        if (t.earth > 0) s += $"{t.earth}  ";
        if (t.grass > 0) s += $"{t.grass}  ";
        return string.IsNullOrEmpty(s) ? "Free" : s.Trim();
    }

    public FlowerDef GetDef(FlowerKind k)
    {
        switch (k)
        {
            case FlowerKind.WaterFlower: return WaterFlower();
            case FlowerKind.EarthFlower: return EarthFlower();
            case FlowerKind.SunFlower: return SunFlower();
            case FlowerKind.GrassFlower: return GrassFlower();
            default: return WaterFlower();

        }
    }
     public void RefreshAllItems()
    {
        var items = GetComponentsInChildren<StoreItem>(true);
        foreach (var it in items) it.RefreshState();
    }
    

    
}
