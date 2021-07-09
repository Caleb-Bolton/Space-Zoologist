using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Store section for food source items.
/// </summary>
public class FoodSourceStoreSection : StoreSection
{
    [SerializeField] FoodSourceManager FoodSourceManager = default;
    private BuildBufferManager buildBufferManager;
    private Color constructionColor = new Color(0.5f, 0.5f, 1f, 1f);//Green

    public override void Initialize()
    {
        this.buildBufferManager = FindObjectOfType<BuildBufferManager>();
        base.itemType = ItemType.Food;
        base.Initialize();
    }

    public void ManuallyPlaceItem(Item item, Vector3Int mouseGridPosition)
    {
        selectedItem = item;
        PlaceFood(mouseGridPosition);
    }

    /// <summary>
    /// Handles the click release on the cursor item.
    /// </summary>
    public override void OnCursorPointerUp(PointerEventData eventData)
    {
        Debug.Log("Attempting to place food");
        base.OnCursorPointerUp(eventData);
        if (base.IsCursorOverUI(eventData) || eventData.button == PointerEventData.InputButton.Right ||
            base.playerBalance.Balance < selectedItem.Price || base.ResourceManager.CheckRemainingResource(selectedItem) == 0)
        {
            Debug.Log("Cannot place item that location");
            base.OnItemSelectionCanceled();
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            PlaceFood(mousePosition);
        }
    }

    public void PlaceFood(Vector3 mousePosition)
    {
        if (!base.GridSystem.PlacementValidation.IsFoodPlacementValid(mousePosition, base.selectedItem))
        {
            Debug.Log("Cannot place item that location");
            return;
        }
        base.playerBalance.SubtractFromBalance(selectedItem.Price);
        base.ResourceManager.Placed(selectedItem, 1);
        base.HandleAudio();
        Vector3Int mouseGridPosition = base.GridSystem.Grid.WorldToCell(mousePosition);
        this.buildBufferManager.CreateBuffer(new Vector2Int(mouseGridPosition.x, mouseGridPosition.y), this.selectedItem.buildTime, this.constructionColor);
        FoodSourceManager.placeFood(mouseGridPosition, base.GridSystem.PlacementValidation.GetFoodSpecies(selectedItem));
    }
}
