using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public Grid board;
    public ResourceManagement bank;
    FlowerAbility current;

    public Sprite WaterResourceIcon;
    public Sprite SunResourceIcon;
    public Sprite EarthResourceIcon;
    public Sprite GrassResourceIcon;
    public GameObject ResourceAmountPrefab;

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

  

    public void RefreshAllItems()
    {
        var items = GetComponentsInChildren<StoreItem>(true);
        foreach (var it in items) it.RefreshState();
    }

    public bool TryApplyOnTile(FlowerAbility ab, GamePiece piece, int qty = 1)
    {
        if (ab == null || board == null || bank == null || piece == null) return false;

        var total = Cost.Multiply(ab.price, qty);


        if (!bank.Has(total))
        {
            Debug.LogWarning("Not enough resources to apply this ability.");
            return false;
        }

        StartCoroutine(board.ApplyAbility(ab, piece)); 
        bank.Spend(total);               

        return true;
    }

    

    
}
