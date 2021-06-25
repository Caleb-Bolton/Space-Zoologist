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

    public override void Initialize()
    {
        base.itemType = ItemType.Food;
        base.Initialize();
    }

    public void ManuallyPlaceItem(Item item, Vector3Int mouseGridPosition)
    {
        selectedItem = item;
        placeFood(mouseGridPosition);
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
            if (!base.GridSystem.PlacementValidation.IsFoodPlacementValid(mousePosition, base.selectedItem))
            {
                Debug.Log("Cannot place item that location");
                return;
            }
            base.playerBalance.SubtractFromBalance(selectedItem.Price);
            base.ResourceManager.Placed(selectedItem, 1);
            Vector3Int mouseGridPosition = base.GridSystem.Grid.WorldToCell(mousePosition);
            placeFood(mouseGridPosition);
        }
    }
    
    public void PlaceFood(Vector3 mousePosition)
    {
        FoodSourceSpecies species = base.GridSystem.PlacementValidation.GetFoodSpecies(selectedItem);
        Vector3Int Temp = mouseGridPosition;
        Temp.x += 1;
        Temp.y += 1;
        GameObject Food;
        Vector3 FoodLocation;
        if (species.Size % 2 == 1)
        {
            //size is odd: center it
            FoodLocation = base.GridSystem.Grid.CellToWorld(mouseGridPosition);
            FoodLocation += Temp;
            FoodLocation /= 2f;
        }
        else
        {
            //size is even: place it at cross-center (position of tile)
            FoodLocation = base.GridSystem.Grid.CellToWorld(Temp);
        }
        base.playerBalance.SubtractFromBalance(selectedItem.Price);
        base.ResourceManager.Placed(selectedItem, 1);
        base.HandleAudio();
        base.audioSource.Play();
        Vector3Int mouseGridPosition = base.GridSystem.Grid.WorldToCell(mousePosition);
        FoodSourceManager.PlaceFood(mouseGridPosition, base.GridSystem.PlacementValidation.GetFoodSpecies(selectedItem), this.selectedItem.buildTime);
    }
}
