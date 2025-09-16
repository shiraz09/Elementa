using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class StoreItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public StoreManager store;
    public FlowerAbility ability;

    public Button iconButton;
    public Image iconImage;

    public Transform PriceContainer;

    public Color canBuyColor = Color.white;
    public Color cantBuyColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Drag & Drop")]
    public Canvas mainCanvas;
    public Image dragIconPrefab;
    public Sprite abilityIconSprite;
    public GraphicRaycaster raycaster;
    private Image draggingIconInstance;
    private RectTransform draggingRT;

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

    // ——— Begin Drag ———
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ability == null || store == null || store.bank == null) return;

        if (!store.bank.Has(ability.price)) return;
        if (mainCanvas == null || dragIconPrefab == null) return;

        draggingIconInstance = Instantiate(dragIconPrefab, mainCanvas.transform);
        draggingRT = draggingIconInstance.rectTransform;
        draggingIconInstance.raycastTarget = false;
        if (abilityIconSprite != null) draggingIconInstance.sprite = abilityIconSprite;

        UpdateDragIconPosition(eventData);
    }
    // ——— Drag ———
    public void OnDrag(PointerEventData eventData)
    {
        if (draggingRT == null) return;
        UpdateDragIconPosition(eventData);
    }
    private void UpdateDragIconPosition(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)mainCanvas.transform, e.position, mainCanvas.worldCamera, out var p);
        draggingRT.anchoredPosition = p;
    }
    // ——— End Drag (Drop) ———
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingIconInstance != null)
            Destroy(draggingIconInstance.gameObject);

        if (raycaster == null || ability == null || store == null) return;

        var results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        GamePiece targetGP = null;

        foreach (var r in results)
        {
            
            targetGP = r.gameObject.GetComponentInParent<GamePiece>();
            if (targetGP != null) break;

            
            var wrapper = r.gameObject.GetComponentInParent<Piece>();
            if (wrapper != null)
            {
                targetGP = wrapper.GetComponent<GamePiece>();
                if (targetGP != null) break;
            }
        }

        if (targetGP != null)
        {
            bool applied = store.TryApplyOnTile(ability, targetGP, 1);
            if (!applied) Debug.LogWarning("Drop found tile but ability failed/applied=false.");
        }
        else
        {
            Debug.Log("Drop did not hit a GamePiece. No charge.");
        }




    }
}



