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
        base.OnCursorPointerUp(eventData);
        if (base.IsCursorOverUI(eventData) || ResourceManager.CheckRemainingResource(base.selectedItem) <= 0)
        {
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
            // base.playerBalance.SubtractFromBalance(selectedItem.Price);
            base.ResourceManager.Placed(selectedItem, 1);
            Vector3Int mouseGridPosition = base.GridSystem.Grid.WorldToCell(mousePosition);
            placeFood(mouseGridPosition);
        }
    }

    // TODO setup manual food placement while Virgil gets saving and loading updated.

    public void placeFood(Vector3Int mouseGridPosition)
    {
        Vector3Int pos;
        Vector2 mousePosition = new Vector2(mouseGridPosition.x, mouseGridPosition.y);
        //base.GridSystem.CellGrid[mouseGridPosition.x, mouseGridPosition.y].Food = FoodSourceManager.CreateFoodSource(selectedItem.ID, mousePosition);
        FoodSourceSpecies species = base.GridSystem.PlacementValidation.GetFoodSpecies(selectedItem);
        int radius = species.Size / 2;
        if (species.Size % 2 == 1)
        {
            //size is odd: center it
            Vector3 FoodLocation = base.GridSystem.Grid.CellToWorld(mouseGridPosition);
            Vector3Int Temp = mouseGridPosition;
            Temp.x += 1;
            Temp.y += 1;
            FoodLocation += Temp;
            FoodLocation /= 2f;

            GameObject Food = FoodSourceManager.CreateFoodSource(selectedItem.ID, FoodLocation);

            // Check if the whole object is in bounds
            for (int x = -1 * radius; x <= radius; x++)
            {
                for (int y = -1 * radius; y <= radius; y++)
                {
                    pos = mouseGridPosition;
                    pos.x += x;
                    pos.y += y;
                    base.GridSystem.CellGrid[pos.x, pos.y].ContainsFood = true;
                    base.GridSystem.CellGrid[pos.x, pos.y].Food = Food;
                }
            }

        }
        else
        {
            //size is even: place it at cross-center (position of tile)
            Vector3Int Temp = mouseGridPosition;
            Temp.x += 1;
            Temp.y += 1;
            Vector3 FoodLocation = base.GridSystem.Grid.CellToWorld(Temp);
            GameObject Food = FoodSourceManager.CreateFoodSource(selectedItem.ID, FoodLocation);

            // Check if the whole object is in bounds
            for (int x = -1 * (radius - 1); x <= radius; x++)
            {
                for (int y = -1 * (radius - 1); y <= radius; y++)
                {
                    pos = mouseGridPosition;
                    pos.x += x;
                    pos.y += y;
                    base.GridSystem.CellGrid[pos.x, pos.y].ContainsFood = true;
                    base.GridSystem.CellGrid[pos.x, pos.y].Food = Food;
                }
            }
        }
    }
}
