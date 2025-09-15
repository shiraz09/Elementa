using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public Grid board;
    public ResourceManagement bank;
    FlowerAbility current;
    
    public bool showAsModal = false;
    //public GameObject overlay; 
    Transform infoOriginalParent;   // to restore parent on close

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

    public void TryBuy(FlowerAbility ab, int qty = 1)
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
    
     public void RefreshAllItems()
    {
        var items = GetComponentsInChildren<StoreItem>(true);
        foreach (var it in items) it.RefreshState();
    }
    

    
}
