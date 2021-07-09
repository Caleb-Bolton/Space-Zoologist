using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

/// <summary>
/// This script is to attached to the inspector button and handles
/// entering/exiting inspect mode, getting 
/// </summary>
public class Inspector : MonoBehaviour
{
    public bool IsInInspectorMode { get; private set; }

    [SerializeField] private GridSystem gridSystem = null;
    [SerializeField] private TileSystem tileSystem = null;
    [SerializeField] private EnclosureSystem enclosureSystem = null;

    [SerializeField] private Tilemap highLight = default;
    // [SerializeField] private GameTile highLightTile = default;

    // The inspector window
    [SerializeField] private GameObject areaDropdownMenu = null;
    [SerializeField] private GameObject itemDropdownMenu = null;
    [SerializeField] private GameObject inspectorWindow = null;
    [SerializeField] private Text inspectorWindowText = null;
    [SerializeField] private GameObject ObjectivePane = null;

    private GameObject lastFoodSourceSelected = null;
    private GameObject lastPopulationSelected = null;
    private List<Vector3Int> lastTilesSelected = new List<Vector3Int>();
    private Dropdown enclosedAreaDropdown;
    private Dropdown itemsDropdown;
    private DisplayInspectorText inspectorWindowDisplayScript;
    private GameObject PopulationHighlighted = null;
    private Vector3Int selectedPosition;

    //TODO This does not feels right to be here
    private List<Life> itemsInEnclosedArea = new List<Life>();

    private void Start()
    {
        this.IsInInspectorMode = false;
        this.enclosedAreaDropdown = this.areaDropdownMenu.GetComponent<Dropdown>();
        this.itemsDropdown = this.itemDropdownMenu.GetComponent<Dropdown>();
        this.enclosedAreaDropdown.onValueChanged.AddListener(selectEnclosedArea);
        this.itemsDropdown.onValueChanged.AddListener(selectItem);
        this.inspectorWindowDisplayScript = this.inspectorWindow.GetComponent<DisplayInspectorText>();

        // Have the dropdown options be refreshed when new items created
        EventManager.Instance.SubscribeToEvent(EventType.NewEnclosedArea, this.UpdateDropdownMenu);
        EventManager.Instance.SubscribeToEvent(EventType.NewFoodSource, this.UpdateDropdownMenu);
        EventManager.Instance.SubscribeToEvent(EventType.NewPopulation, this.UpdateDropdownMenu);
    }

    public void CloseInspector()
    {
        if (this.IsInInspectorMode)
        {
            this.inspectorWindow.SetActive(false);
            //this.areaDropdownMenu.SetActive(false);
            //this.itemDropdownMenu.SetActive(false);
            //this.HUD.SetActive(true);
            this.UnHighlightAll();
            EventManager.Instance.InvokeEvent(EventType.InspectorClosed, null);
            this.IsInInspectorMode = !IsInInspectorMode;
            AudioManager.instance.PlayOneShot(SFXType.MenuClose);
        }

    }

    public void OpenInspector()
    {
        this.inspectorWindowText.text = "Click on a point of interest to gather information";
        this.inspectorWindow.SetActive(true);
        this.gridSystem.UpdateAnimalCellGrid();
        //this.UpdateDropdownMenu();
        //this.areaDropdownMenu.SetActive(true);
        //this.itemDropdownMenu.SetActive(true);
        //this.HUD.SetActive(false);
        EventManager.Instance.InvokeEvent(EventType.InspectorOpened, null);
        this.IsInInspectorMode = !IsInInspectorMode;
        AudioManager.instance.PlayOneShot(SFXType.MenuOpen);
    }

    /// <summary>
    /// Toggle displays
    /// </summary>
    public void ToggleInspectMode()
    {

        // Toggle button text, displays and pause/free animals
        if (!this.IsInInspectorMode)
        {
            this.OpenInspector();
        }
        else
        {
            this.CloseInspector();
        }

        //Debug.Log($"Inspector mode is {this.IsInInspectorMode}");
    }

    private void UpdateDropdownMenu()
    {
        this.enclosedAreaDropdown.options.Clear();

        this.itemsDropdown.options.Clear();
        this.itemsInEnclosedArea.Clear();
        this.itemsDropdown.options.Add(new Dropdown.OptionData { text = $"Select an item" });

        // Add empty option
        this.enclosedAreaDropdown.options.Add(new Dropdown.OptionData { text = $"Select an area" });

        foreach (EnclosedArea enclosedArea in this.enclosureSystem.EnclosedAreas)
        {
            this.enclosedAreaDropdown.options.Add(new Dropdown.OptionData { text = $"Enclosed Area {enclosedArea.id}"});
        }
    }

    private void selectEnclosedArea(int selection)
    {
        // Selected placeholder option
        if (selection == 0)
        {
            return;
        }

        EnclosedArea enclosedAreaSelected = this.enclosureSystem.EnclosedAreas[selection-1];

        //Debug.Log($"Enclosed area {enclosedAreaSelected.id} selected from dropdown");

        this.itemsDropdown.options.Clear();
        this.itemsInEnclosedArea.Clear();

        this.itemsDropdown.options.Add(new Dropdown.OptionData { text = $"Select an item" });

        foreach (Population population in enclosedAreaSelected.populations)
        {
            this.itemsDropdown.options.Add(new Dropdown.OptionData { text = $"{population.Species.SpeciesName}" });
            this.itemsInEnclosedArea.Add(population);
        }

        foreach (FoodSource foodSource in enclosedAreaSelected.foodSources)
        {
            this.itemsDropdown.options.Add(new Dropdown.OptionData { text = $"{foodSource.Species.SpeciesName}" });
            this.itemsInEnclosedArea.Add(foodSource);
        }

        // Set item selection to placeholder option
        this.itemsDropdown.value = 0;

        this.inspectorWindowDisplayScript.DislplayEnclosedArea(enclosedAreaSelected);
    }

    private void selectItem(int selection)
    {
        // Selected placeholder option
        if (selection == 0)
        {
            return;
        }

        //Debug.Log($"selected item {selection} from dropdown");

        Life itemSelected = this.itemsInEnclosedArea[selection-1];

        if (itemSelected.GetType() == typeof(Population))
        {
            this.HighlightPopulation(((Population)itemSelected).gameObject);
            this.inspectorWindowDisplayScript.DisplayPopulationStatus((Population)itemSelected);
        }
        if (itemSelected.GetType() == typeof(FoodSource))
        {
            this.HighlightFoodSource(((FoodSource)itemSelected).gameObject);
            this.inspectorWindowDisplayScript.DisplayFoodSourceStatus((FoodSource)itemSelected);
        }

        // Set enclosed area dropdown to placeholder selection
        this.enclosedAreaDropdown.value = 0;
    }

    /// <summary>
    /// Listens to mouse clicks and what was selected, then call functions to
    /// display info on inspector window
    /// </summary>
    public void Update()
    {
        if (this.IsInInspectorMode && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.layer == 5)
            {
                return;
            }
            this.UpdateInspectorValues();
        }
        if (this.IsInInspectorMode)
        {
            if (this.PopulationHighlighted != null)
            {
                this.HighlightPopulation(this.PopulationHighlighted);
            }
        }
    }

    // TODO break this up and refactor
    public void UpdateInspectorValues()
    {
        // Update animal location reference
        this.gridSystem.UpdateAnimalCellGrid();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = this.tileSystem.WorldToCell(worldPos);
        GameTile tile = this.tileSystem.GetGameTileAt(pos);
        GridSystem.CellData cellData = getCellData(pos);
        if (cellData.OutOfBounds)
        {
            return;
        }

        this.UnHighlightAll();
        if (cellData.ContainsAnimal)
        {
            DisplayPopulationText(cellData);
            selectedPosition = pos;
        }
        // Selection is food source or item
        else if (cellData.ContainsFood)
        {
            DisplayFoodText(cellData);
            selectedPosition = pos;
        }
        // Selection is liquid tile
        else if (tile && tile.type == TileType.Liquid)
        {
            DisplayLiquidText(pos);
            selectedPosition = pos;
        }
        // Selection is enclosed area
        else if (tile && tile.type != TileType.Wall)
        {
            DisplayAreaText(pos);
            selectedPosition = pos;
        }
        AudioManager.instance.PlayOneShot(SFXType.Notification);

        // Reset dropdown selections
        this.enclosedAreaDropdown.value = 0;
        this.itemsDropdown.value = 0;
    }

    private GridSystem.CellData getCellData(Vector3Int cellPos)
    {
        GridSystem.CellData cellData = new GridSystem.CellData();
        // Handles index out of bound exception
        if (this.gridSystem.isCellinGrid(cellPos.x, cellPos.y))
        {
            cellData = this.gridSystem.CellGrid[cellPos.x, cellPos.y];
        }
        else
        {
            cellData.OutOfBounds = true;
        }
        return cellData;
    }

    public void UpdateCurrentDisplay()
    {
        GridSystem.CellData cellData = getCellData(selectedPosition);
        switch (inspectorWindowDisplayScript.CurrentDisplay)
        {
            case DisplayInspectorText.InspectorText.Population:
                if (!cellData.ContainsAnimal)
                {
                    return;
                }
                DisplayPopulationText(cellData);
                break;
            case DisplayInspectorText.InspectorText.Food:
                if (!cellData.ContainsFood)
                {
                    return;
                }
                DisplayFoodText(cellData);
                break;
            case DisplayInspectorText.InspectorText.Area:
                DisplayAreaText(selectedPosition);
                break;
            case DisplayInspectorText.InspectorText.Liquid:
                DisplayLiquidText(selectedPosition);
                break;
            default:
                break;
        }
    }

    private void DisplayPopulationText(GridSystem.CellData cellData)
    {
        this.HighlightPopulation(cellData.Animal.transform.parent.gameObject);
        //Debug.Log($"Found animal {cellData.Animal.GetComponent<Animal>().PopulationInfo.Species.SpeciesName} @ {cellPos}");
        this.inspectorWindowDisplayScript.DisplayPopulationStatus(cellData.Animal.GetComponent<Animal>().PopulationInfo);
    }

    private void DisplayFoodText(GridSystem.CellData cellData)
    {
        this.HighlightFoodSource(cellData.Food);
        //Debug.Log($"Foudn item {cellData.Food} @ {cellPos}");
        this.inspectorWindowDisplayScript.DisplayFoodSourceStatus(cellData.Food.GetComponent<FoodSource>());
    }

    private void DisplayAreaText(Vector3Int cellPos)
    {
        this.enclosureSystem.UpdateEnclosedAreas();
        this.inspectorWindowDisplayScript.DislplayEnclosedArea(this.enclosureSystem.GetEnclosedAreaByCellPosition(cellPos));
        //Debug.Log($"Enclosed are @ {cellPos} selected");
    }

    private void DisplayLiquidText(Vector3Int cellPos)
    {
        //Debug.Log($"Selected liquid tile @ {cellPos}");
        float[] compositions = this.tileSystem.GetLiquidBodyAt(cellPos).contents;
        this.inspectorWindowDisplayScript.DisplayLiquidCompisition(compositions);
    }


    private void UnHighlightAll()
    {
        if( this.lastFoodSourceSelected)
        {
            this.lastFoodSourceSelected.GetComponent<SpriteRenderer>().color = Color.white;
            this.lastFoodSourceSelected = null;
        }
        if (this.lastPopulationSelected)
        {
            this.PopulationHighlighted = null;
            foreach (Transform child in this.lastPopulationSelected.transform)
            {
                child.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
            this.lastPopulationSelected = null;
        }

        foreach (Vector3Int pos in this.lastTilesSelected)
        {
            this.highLight.SetTile(pos, null);
        }
    }

    private void HighlightPopulation(GameObject population)
    {
        this.PopulationHighlighted = population;
        foreach (Transform child in population.transform)
        {
            child.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        this.lastPopulationSelected = population;
    }


    private void HighlightFoodSource(GameObject foodSourceGameObject)
    {
        // Highlight food source object
        foodSourceGameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        this.lastFoodSourceSelected = foodSourceGameObject;
    }

    public GameObject GetAnimalSelected()
    {
        return lastPopulationSelected;
    }
}