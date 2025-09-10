using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StoreItem : MonoBehaviour
{
    public StoreManager store;
    public StoreManager.FlowerKind kind;

    public Button iconButton;
    public Image iconImage;
    public TMP_Text titleLabel;
    public TMP_Text priceLabel;
    public TMP_Text effectPreviewLabel;
    public Button infoButton;
    public Button buyButtonInCard;
    private StoreManager.FlowerDef def;
    public Color canBuyColor = Color.white;
    public Color cantBuyColor = new Color(1f, 1f, 1f, 0.35f);
    void Awake()
    {
        if (!store) store = GetComponentInParent<StoreManager>();
        if (!iconButton) iconButton = GetComponent<Button>();
        if (!iconImage && iconButton) iconImage = iconButton.GetComponent<Image>();
    }
    void Start()
    {
        def = store.GetDef(kind);
        if (titleLabel) titleLabel.text = def.title;
        if (priceLabel) priceLabel.text = store.FormatCost(def.price);
        if (effectPreviewLabel) effectPreviewLabel.text = def.description;
        if (iconButton)
            iconButton.onClick.AddListener(() =>
                store.ShowInfo(def, (RectTransform)iconButton.transform));
        int qty = 1;
        if (infoButton)
            infoButton.onClick.AddListener(() =>
                store.ShowInfo(def, (RectTransform)iconButton.transform, qty));

        if (buyButtonInCard)
            buyButtonInCard.onClick.AddListener(() =>
                store.TryBuy(def));
        RefreshState();

    }
    public void RefreshState()
    {
        bool canBuy = store.bank && store.bank.Has(def.price);

        if (buyButtonInCard) buyButtonInCard.interactable = canBuy;


        if (iconImage) iconImage.color = canBuy ? canBuyColor : cantBuyColor;


        Color txt = canBuy ? Color.white : new Color(1, 1, 1, 0.6f);
        if (titleLabel) titleLabel.color = txt;
        if (priceLabel) priceLabel.color = txt;
        if (effectPreviewLabel) effectPreviewLabel.color = txt;
    }
}


