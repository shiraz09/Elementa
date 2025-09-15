using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StoreItem : MonoBehaviour
{
    public StoreManager store;
    public FlowerAbility ability;

    public Button iconButton;
    public Image iconImage;

    public Transform PriceContainer;
    
    public Color canBuyColor = Color.white;
    public Color cantBuyColor = new Color(1f, 1f, 1f, 0.35f);

    void Awake()
    {
        if (!store) store = GetComponentInParent<StoreManager>();
        if (!iconImage && iconButton) iconImage = iconButton.GetComponent<Image>();
        iconButton.onClick.AddListener(BuildFromAbility);

        if (ability.price.water > 0)
        {
            var resourceAmount = Instantiate(store.ResourceAmountPrefab, PriceContainer);
            resourceAmount.GetComponentInChildren<Image>().sprite = store.WaterResourceIcon;
            resourceAmount.GetComponentInChildren<TMP_Text>().text = ability.price.water.ToString();
        }

        if (ability.price.sun > 0)
        {
            var resourceAmount = Instantiate(store.ResourceAmountPrefab, PriceContainer);
            resourceAmount.GetComponentInChildren<Image>().sprite = store.SunResourceIcon;
            resourceAmount.GetComponentInChildren<TMP_Text>().text = ability.price.sun.ToString();
        }

        if (ability.price.earth > 0)
        {
            var resourceAmount = Instantiate(store.ResourceAmountPrefab, PriceContainer);
            resourceAmount.GetComponentInChildren<Image>().sprite = store.EarthResourceIcon;
            resourceAmount.GetComponentInChildren<TMP_Text>().text = ability.price.earth.ToString();
        }

        if (ability.price.grass > 0)
        {
            var resourceAmount = Instantiate(store.ResourceAmountPrefab, PriceContainer);
            resourceAmount.GetComponentInChildren<Image>().sprite = store.GrassResourceIcon;
            resourceAmount.GetComponentInChildren<TMP_Text>().text = ability.price.grass.ToString();
        }
    }

    void OnEnable()
    {
        if (store && store.bank != null) store.bank.OnChanged += RefreshState;
        RefreshState();
    }
    void OnDisable()
    {
        if (store && store.bank != null) store.bank.OnChanged -= RefreshState;
    }

    private void BuildFromAbility()
    {
        if (ability == null || store == null) return;

        if (iconButton != null)
        {
            store.TryBuy(ability);
        }
    }

    public void RefreshState()
    {
        if (store == null || store.bank == null || ability == null) return;
        bool canBuy = store.bank.Has(ability.price);

        if (iconImage)
            iconImage.color = canBuy ? canBuyColor : cantBuyColor;

        Color txt = canBuy ? Color.white : new Color(1f, 1f, 1f, 0.6f);
    }
}


