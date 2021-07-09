
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// TODO setup player balance to show remaining items and strobe when out
/// <summary>
/// A section of items in the store. Subclass for specific behavior regarding what happens after an item is selected.
/// </summary>
public class StoreSection : MonoBehaviour
{
    public ItemType ItemType => itemType;

    protected ItemType itemType = default;
    [Header("Dependencies")]
    [SerializeField] private Transform itemGrid = default;
    [SerializeField] private GameObject itemCellPrefab = default;
    protected CanvasObjectStrobe PlayerBalanceDisplay = default;
    protected CursorItem cursorItem = default;
    protected List<RectTransform> UIElements = default;
    protected PlayerBalance playerBalance = default;
    protected LevelDataReference LevelDataReference = default;
    protected GridSystem GridSystem = default;
    protected ResourceManager ResourceManager = default;
    private Dictionary<Item, StoreItemCell> storeItems = new Dictionary<Item, StoreItemCell>();
    private GridOverlay gridOverlay = default;
    protected Item selectedItem = null;
    private Vector3Int previousLocation = default;
    protected int currentAudioIndex = 0;

    public void SetupDependencies(LevelDataReference levelData, CursorItem cursorItem, List<RectTransform> UIElements, GridSystem gridSystem, PlayerBalance playerBalance, CanvasObjectStrobe playerBalanceDisplay, ResourceManager resourceManager)
    {
        this.LevelDataReference = levelData;
        this.cursorItem = cursorItem;
        this.UIElements = UIElements;
        this.GridSystem = gridSystem;
        gridOverlay = GridSystem.gameObject.GetComponent<GridOverlay>();
        this.playerBalance = playerBalance;
        this.PlayerBalanceDisplay = playerBalanceDisplay;
        this.ResourceManager = resourceManager;
    }

    public void Update()
    {
        if (cursorItem.IsOn)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(cursorItem.transform.position);
            if (!GridSystem.IsWithinGridBounds(mousePosition)) return;

            Vector3Int gridLocation = GridSystem.Grid.WorldToCell(mousePosition);
            if (this.GridSystem.PlacementValidation.IsOnWall(gridLocation)) return;

            if (gridLocation.x != previousLocation.x || gridLocation.y != previousLocation.y)
            {
                previousLocation = gridLocation;
                gridOverlay.ClearColors();
                GridSystem.PlacementValidation.updateVisualPlacement(gridLocation, selectedItem);
            }
        }
    }

    public virtual void Initialize()
    {
        LevelData levelData = LevelDataReference.LevelData;
        foreach (Item item in levelData.Items)
        {
            if (item.Type.Equals(itemType))
            {
                this.AddItem(item);
            }
        }
    }

    /// <summary>
    /// Add item to the section.
    /// </summary>
    public void AddItem(Item item)
    {
        GameObject newItemCellGO = Instantiate(itemCellPrefab, itemGrid);
        StoreItemCell itemCell = newItemCellGO.GetComponent<StoreItemCell>();
        itemCell.Initialize(item, OnItemSelected);
        if (this.ResourceManager.hasLimitedSupply(item.ItemName))
        {
            this.ResourceManager.setupItemSupplyTracker(itemCell);
            storeItems.Add(item, itemCell);
        }
    }

    /// <summary>
    /// Triggered by items in the section.
    /// </summary>
    /// <param name="item">The item that was selected.</param>
    public virtual void OnItemSelected(Item item)
    {
        if (!this.HasSupply(item))
        {
            // this.PlayerBalanceDisplay.StrobeColor(2, Color.red);
            return;
        }
        cursorItem.Begin(item.Icon, OnCursorItemClicked, OnCursorPointerDown, OnCursorPointerUp);
        selectedItem = item;
    }

    public virtual void OnItemSelectionCanceled()
    {
        cursorItem.Stop(OnCursorItemClicked, OnCursorPointerDown, OnCursorPointerUp);
        gridOverlay.ClearColors();
    }

    public void OnCursorItemClicked(PointerEventData eventData)
    {
        if (!this.HasSupply(this.selectedItem))
        {
            // this.PlayerBalanceDisplay.StrobeColor(2, Color.red);
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnItemSelectionCanceled();
        }
    }

    public bool HasSupply(Item item)
    {
        if (storeItems.ContainsKey(item) && playerBalance.Balance < storeItems[item].item.Price && ResourceManager.CheckRemainingResource(item) == 0)
        {
            Debug.Log("You can't buy this!");
            OnItemSelectionCanceled();
            return false;
        }
        return true;
    }

    /// <summary>
    /// Overwritten by child classes.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnCursorPointerDown(PointerEventData eventData)
    {

    }

    /// <summary>
    /// Overwritten by child classes.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnCursorPointerUp(PointerEventData eventData)
    {
        if (!this.HasSupply(this.selectedItem))
        {
            // this.PlayerBalanceDisplay.StrobeColor(2, Color.red);
            return;
        }
    }

    public virtual bool IsPlacementValid(Vector3 mousePosition)
    {
        return false;
    }

    private void OnDisable()
    {
        cursorItem.Stop(OnCursorItemClicked, OnCursorPointerDown, OnCursorPointerUp);
    }

    public bool IsCursorOverUI(PointerEventData eventData)
    {
        foreach (RectTransform UIElement in this.UIElements)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(UIElement, eventData.position))
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void HandleAudio()
    {
        if (selectedItem.AudioClips.Count == 0)
        {
            Debug.Log("Selected item " + selectedItem.ItemName + " has no audio sources!");
            return;
        }
        if (selectedItem.AudioClips.Count > 1)
        {
            currentAudioIndex += 1;
            if (currentAudioIndex >= selectedItem.AudioClips.Count)
            {
                currentAudioIndex = 0;
            }
        }
        AudioManager.instance.PlayOneShot(selectedItem.AudioClips[currentAudioIndex]);
    }
}
