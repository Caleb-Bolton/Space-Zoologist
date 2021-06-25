using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Store section for tile items.
/// </summary>
/// Figure out how to handle case when more than one machine present after changes
public class TileStoreSection : StoreSection
{
    [SerializeField] private EnclosureSystem EnclosureSystem = default;
    [SerializeField] private TilePlacementController tilePlacementController = default;

    private float startingBalance;
    private int initialAmt;
    private bool isPlacing = false;
    private int numTilesPlaced = 0;

    private BuildBufferManager buildBufferManager;
    private Color constructionColor = new Color(1f, 0.6f, 0.2f, 1f);//Orange
    public override void Initialize()
    {
        this.buildBufferManager = FindObjectOfType<BuildBufferManager>();
        base.itemType = ItemType.Terrain;
        base.Initialize();
        Debug.Assert(tilePlacementController != null);
    }

    /// <summary>
    /// Start tile placement preview.
    /// </summary>
    private void StartPlacing()
    {
        numTilesPlaced = 0;
        initialAmt = ResourceManager.CheckRemainingResource(selectedItem);
        isPlacing = true;
        startingBalance = base.playerBalance.Balance;
        tilePlacementController.StartPreview(selectedItem.ID);
    }

    /// <summary>
    /// Stop tile placement preview and remove any changes.
    /// </summary>
    private void CancelPlacing()
    {
        isPlacing = false;
        tilePlacementController.RevertChanges();
        base.playerBalance.SetBalance(startingBalance);
    }

    /// <summary>
    /// Stop tile placement preview and finalize changes to the grid.
    /// </summary>
    private void FinishPlacing()
    {
        Debug.Log("Finish placing");
        isPlacing = false;
        foreach (Vector3Int pos in this.tilePlacementController.addedTiles)
        {
            this.buildBufferManager.CreateUnitBuffer(new Vector2Int(pos.x,pos.y), this.selectedItem.buildTime, constructionColor);
        }
        this.EnclosureSystem.UpdateEnclosedAreas();
        tilePlacementController.StopPreview();
        base.playerBalance.SetBalance(startingBalance - numTilesPlaced * selectedItem.Price);
        base.ResourceManager.Placed(selectedItem, numTilesPlaced);
    }

    /// <summary>
    /// Triggered by mouse down on the cursor item.
    /// </summary>
    public override void OnCursorPointerDown(PointerEventData eventData)
    {
        base.OnCursorPointerDown(eventData);
        if (base.IsCursorOverUI(eventData))
        {
            base.OnItemSelectionCanceled();
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left && !isPlacing)
        {
            this.StartPlacing();
        }
    }

    /// <summary>
    /// Triggered by mouse up on the cursor item.
    /// </summary>
    public override void OnCursorPointerUp(PointerEventData eventData)
    {
        base.OnCursorPointerUp(eventData);
        if (eventData.button == PointerEventData.InputButton.Left && isPlacing)
        {
            FinishPlacing();
        }
    }

    /// <summary>
    /// Event when the item selection is canceled.
    /// </summary>
    public override void OnItemSelectionCanceled()
    {
        //Debug.Log("Tile placement canceled");
        base.OnItemSelectionCanceled();
        //CancelPlacing();
    }

    private void Update()
    {
        base.Update();
        if (isPlacing)
        {
            if (this.tilePlacementController.PlacementPaused)
            {
                return;
            }
            else
            {
                numTilesPlaced = tilePlacementController.PlacedTileCount();
                base.playerBalance.SetBalance(startingBalance - numTilesPlaced * selectedItem.Price);

                if (base.playerBalance.Balance < selectedItem.Price || initialAmt - numTilesPlaced == 0)
                {
                    FinishPlacing();
                    base.OnItemSelectionCanceled();
                }
            }
        }
    }

}
