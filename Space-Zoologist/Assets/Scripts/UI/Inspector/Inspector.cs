using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

/// <summary>
/// This script is to attached to the inspector button and handles
/// entering/exiting inspect mode, getting 
/// </summary>
public class Inspector : MonoBehaviour
{
    private bool isInInspectorMode = false;

    [SerializeField] private Text inspectorButtonText = null;

    [SerializeField] private GridSystem gridSystem = null;
    [SerializeField] private TileSystem tileSystem = null;
    [SerializeField] private EnclosureSystem enclosureSystem = null;
    [SerializeField] private PauseManager PauseManager = default;
    [SerializeField] private MenuManager MenuManager = default;

    [SerializeField] private Tilemap highLight = default;
    [SerializeField] private TerrainTile highLightTile = default;

    // To access other UI elements to toggle
    [SerializeField] private GameObject HUD = null;
    // The inspector window
    [SerializeField] private GameObject areaDropdownMenu = null;
    [SerializeField] private GameObject itemDropdownMenu = null;
    [SerializeField] private GameObject inspectorWindow = null;
    [SerializeField] private Text inspectorWindowText = null;

    private GameObject lastFoodSourceSelected = null;
    private GameObject lastPopulationSelected = null;
    private List<Vector3Int> lastTilesSelected = new List<Vector3Int>();
    private Dropdown enclosedAreaDropdown;
    private Dropdown itemsDropdown;
    private DisplayInspectorText inspectorWindowDisplayScript;

    //TODO This does not feels right to be here
    private List<Life> itemsInEnclosedArea = new List<Life>();

    private void Start()
    {
        this.enclosedAreaDropdown = this.areaDropdownMenu.GetComponent<Dropdown>();
        this.itemsDropdown = this.itemDropdownMenu.GetComponent<Dropdown>();
        this.enclosedAreaDropdown.onValueChanged.AddListener(selectEnclosedArea);
        this.itemsDropdown.onValueChanged.AddListener(selectItem);
        this.inspectorWindowDisplayScript = this.inspectorWindow.GetComponent<DisplayInspectorText>();
    }

    /// <summary>
    /// Toggle displays
    /// </summary>
    public void ToggleInspectMode()
    {
        // Cannot enter inspector mode while in istore
        if (this.MenuManager.IsInStore)
        {
            return;
        }

        // Toggle flag
        this.isInInspectorMode = !isInInspectorMode;

        // Toggle button text, displays and pause/free animals
        if (this.isInInspectorMode)
        {
            this.inspectorButtonText.text = "INSPECTOR:ON";
            this.inspectorWindowText.text = "INSPECTOR";
            this.PauseManager.Pause();
            this.inspectorWindow.SetActive(true);
            this.UpdateDropdownMenu();
            this.areaDropdownMenu.SetActive(true);
            this.itemDropdownMenu.SetActive(true);
            this.HUD.SetActive(false);

            EventManager.Instance.InvokeEvent(EventType.InspectorOpened, null);
        }
        else
        {
            this.inspectorButtonText.text = "INSPECTOR:OFF";
            this.PauseManager.Unpause();
            this.inspectorWindow.SetActive(false);
            this.areaDropdownMenu.SetActive(false);
            this.itemDropdownMenu.SetActive(false);
            this.HUD.SetActive(true);
            this.UnHighlightAll();

            EventManager.Instance.InvokeEvent(EventType.InspectorClosed, null);

        }

        //Debug.Log($"Inspector mode is {this.isInInspectorMode}");
    }

    private void UpdateDropdownMenu()
    {
        this.enclosedAreaDropdown.options.Clear();

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

        Debug.Log($"Enclosed area {enclosedAreaSelected.id} selected from dropdown");

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

        Debug.Log($"selected item {selection} from dropdown");

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
        if (this.isInInspectorMode && Input.GetMouseButtonDown(0))
        {
            // Update animal locations
            this.gridSystem.UpdateAnimalCellGrid();

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = this.tileSystem.WorldToCell(worldPos);
            TerrainTile tile = this.tileSystem.GetTerrainTileAtLocation(cellPos);

            //Debug.Log($"Mouse click at {cellPos}");

            GridSystem.CellData cellData;

            // Handles index out of bound exception
            if (this.gridSystem.isCellinGrid(cellPos.x, cellPos.y))
            {
                cellData = this.gridSystem.CellGrid[cellPos.x, cellPos.y];
            }
            else
            {
                Debug.Log($"Grid location selected was out of bounds @ {cellPos}");
                return;
            }

            // Check if selection is anaiaml
            if (cellData.ContainsAnimal)
            {
                this.UnHighlightAll();
                this.HighlightPopulation(cellData.Animal.transform.parent.gameObject);
                //Debug.Log($"Found animal {cellData.Animal.GetComponent<Animal>().PopulationInfo.Species.SpeciesName} @ {cellPos}");
                this.inspectorWindowDisplayScript.DisplayPopulationStatus(cellData.Animal.GetComponent<Animal>().PopulationInfo);
            }
            // Selection is food source or item
            else if (cellData.ContainsFood)
            {
                this.UnHighlightAll();
                this.HighlightFoodSource(cellData.Food);
                //Debug.Log($"Foudn item {cellData.Food} @ {cellPos}");
                this.inspectorWindowDisplayScript.DisplayFoodSourceStatus(cellData.Food.GetComponent<FoodSource>());
            }
            // Selection is liquid tile
            else if (tile.type == TileType.Liquid)
            {
                this.UnHighlightAll();
                this.HighlightSingleTile(cellPos);
                //Debug.Log($"Selected liquid tile @ {cellPos}");
                float[] compositions = this.tileSystem.GetTileContentsAtLocation(cellPos, tile);
                this.inspectorWindowDisplayScript.DisplayLiquidCompisition(compositions);
            }
            // Selection is enclosed area
            else if (tile && tile.type != TileType.Wall)
            {
                this.UnHighlightAll();
                this.HighlightEnclosedArea(cellPos);
                this.enclosureSystem.UpdateEnclosedAreas();
                this.inspectorWindowDisplayScript.DislplayEnclosedArea(this.enclosureSystem.GetEnclosedAreaByCellPosition(cellPos));
                //Debug.Log($"Enclosed are @ {cellPos} selected");
            }

            // Reset dropdown selections
            this.enclosedAreaDropdown.value = 0;
            this.itemsDropdown.value = 0;
        }
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

        FoodSource foodSource = foodSourceGameObject.GetComponent<FoodSource>();

        // Hightlight
        List<Vector3Int> foodSourceRadiusRange = this.tileSystem.AllCellLocationsinRange(Vector3Int.FloorToInt(foodSource.Position), foodSource.Species.RootRadius);
        foreach (Vector3Int pos in foodSourceRadiusRange)
        {
            this.highLight.SetTile(pos, this.highLightTile);
        }

        this.lastTilesSelected = foodSourceRadiusRange;
    }

    

    private void HighlightEnclosedArea(Vector3Int selectedLocation)
    {

    }

    private void UnhighlightEnclosedArea(Vector3Int selectedLocation)
    {

    }

    private void UnHighSignleTile(Vector3Int location)
    {

    }

    private void HighlightSingleTile(Vector3Int location)
    {

    }
}