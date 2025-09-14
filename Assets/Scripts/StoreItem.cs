using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StoreItem : MonoBehaviour
{
    public StoreManager store;
    public BoardAbility ability;

    public Button iconButton;
    public Image iconImage;
    public TMP_Text titleLabel;
    public TMP_Text priceLabel;
    public TMP_Text effectPreviewLabel;
    public Button infoButton;
    public Button buyButtonInCard;

    public Color canBuyColor = Color.white;
    public Color cantBuyColor = new Color(1f, 1f, 1f, 0.35f);
    void Awake()
    {
        if (!store) store = GetComponentInParent<StoreManager>();
        if (!iconImage && iconButton) iconImage = iconButton.GetComponent<Image>();
    }
    void OnEnable()
    {
        if (store && store.bank != null) store.bank.OnChanged += RefreshState;
        BuildFromAbility();
        RefreshState();
    }
    void OnDisable()
    {
        if (store && store.bank != null) store.bank.OnChanged -= RefreshState;
    }


    private void BuildFromAbility()
    {
        if (ability == null || store == null) return;

        if (titleLabel)
            titleLabel.text = ability.title;
        if (priceLabel)
            priceLabel.text = store.FormatCost(ability.price);
        if (effectPreviewLabel)
            effectPreviewLabel.text = ability.description;
        if (iconImage && ability.icon)
            iconImage.sprite = ability.icon;

        if (iconButton)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(() =>
                store.ShowInfo(ability, (RectTransform)iconButton.transform));
        }

        if (infoButton)
        {
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(() =>
                store.ShowInfo(ability, (RectTransform)(iconButton ? iconButton.transform : transform)));
        }

        if (buyButtonInCard)
        {
            buyButtonInCard.onClick.RemoveAllListeners();
            buyButtonInCard.onClick.AddListener(() => store.TryBuy(ability));
        }
    }
    
    public void RefreshState()
    {
        if (store == null || store.bank == null || ability == null) return;
        bool canBuy = store.bank.Has(ability.price);


        if (buyButtonInCard)
            buyButtonInCard.interactable = canBuy;
        if (iconImage)
            iconImage.color = canBuy ? canBuyColor : cantBuyColor;

        Color txt = canBuy ? Color.white : new Color(1f, 1f, 1f, 0.6f);
        if (titleLabel)
            titleLabel.color = txt;
        if (priceLabel)
            priceLabel.color = txt;
        if (effectPreviewLabel)
            effectPreviewLabel.color = txt;
        }
}


