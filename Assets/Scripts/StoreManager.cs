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

    class FlowerDef {
        public string title;
        public Cost price;
        public Grid.TargetMode mode;
        public string description;
    }
    FlowerDef current;
    FlowerDef blue() => new FlowerDef
    {
        title = "Blue Flower",
        price = new Cost { water = 10 },
        mode = Grid.TargetMode.Row,
        description = "Clears a row"
    };
    FlowerDef red() => new FlowerDef
    {
        title = "Red Flower",
        price = new Cost { earth = 3, water = 1, sun = 1 },
        mode = Grid.TargetMode.Column,
        description = "Clears a column"
    };
    FlowerDef Pink() => new FlowerDef
    {
        title = "Pink Flower",
        price = new Cost { water = 3, sun = 3, grass = 3, earth = 3 },
        mode = Grid.TargetMode.AllOfType,
        description = "Clears all of one type"
    };
    FlowerDef Green() => new FlowerDef
    {
        title = "Green Flower",
        price = new Cost { grass = 2, water = 2, sun = 2 },
        mode = Grid.TargetMode.Cell3x3,
        description = "Clears a 3x3 area"
    };


    void Awake()
    {

        btnBlue.onClick.AddListener(() => ShowInfo(blue()));
        btnRed.onClick.AddListener(() => ShowInfo(red()));
        btnPink.onClick.AddListener(() => ShowInfo(Pink()));
        btnGreen.onClick.AddListener(() => ShowInfo(Green()));
        buyButton.onClick.AddListener(TryBuy);
        closeButton.onClick.AddListener(() => infoPanel.SetActive(false));
        infoPanel.SetActive(false);
    }
    void ShowInfo(FlowerDef def){
        current = def;
        titleText.text = def.title;
        costText.text  = FormatCost(def.price);
        effectText.text= def.description;
        infoPanel.SetActive(true);
    }
    void TryBuy(){
        if (current==null || bank==null || board==null) return;

        if (!bank.Spend(current.price)){
            Debug.Log("Not enough resources");
            return;
        }
        board.EnterTargetMode(current.mode);
        infoPanel.SetActive(false);
    }
    string FormatCost(Cost c){
        string s="";
        if (c.water>0) s += $"{c.water}  ";
        if (c.sun>0)   s += $"{c.sun} ";
        if (c.earth>0) s += $"{c.earth}  ";
        if (c.grass>0) s += $"{c.grass}  ";
        return string.IsNullOrEmpty(s) ? "Free" : s.Trim();
    }

    


    
}
