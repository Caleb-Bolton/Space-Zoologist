using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
enum SelectionType {Tile, Animal, FoodSource, None }
public class MapDesigningTool : MonoBehaviour
{
    // Start is called before the first frame update
    private LevelDataReference LevelDataReference;
    private TileType selectedTile;
    private AnimalSpecies selectedSpecies;
    private FoodSourceSpecies selectedFood;
    private SelectionType selectionType;
    private Vector2 tileScrollPosition;
    private string sceneName;
    private LevelIO levelIO;
    private TilePlacementController tilePlacementController;
    private FoodSourceManager foodSourceManager;
    private PopulationManager populationManager;
    [SerializeField] bool godMode = true;
    private FoodSourceSpecies[] foodSourceSpecies = default; //Attach food sources here
    private bool DisplayLiquidBodyInfo = true;
    private bool DisplayPreviewBodies;
    private Tilemap[] tilemaps;
    private TileSystem tileSystem;
    private Vector2 liquidScrollPos;
    private Vector2 previewBodyScrollPos;
    private Vector2 animalScrollPos;
    private Vector2 foodScrollPos;
    private Camera mainCamera;
    private Dictionary<TileLayerManager, Dictionary<LiquidBody, bool>> ManagersToToggles = new Dictionary<TileLayerManager, Dictionary<LiquidBody, bool>>();
    private void Awake()
    {
        this.levelIO = FindObjectOfType<LevelIO>();
        this.tilePlacementController = FindObjectOfType<TilePlacementController>();
        this.mainCamera = this.gameObject.GetComponent<Camera>();
        this.tilemaps = FindObjectsOfType<Tilemap>();
        this.tileSystem = FindObjectOfType<TileSystem>();
        this.populationManager = FindObjectOfType<PopulationManager>();
        this.foodSourceManager = FindObjectOfType<FoodSourceManager>();
        this.LevelDataReference = FindObjectOfType<LevelDataReference>();
        // Load food sources from leveldata.
        // Loading other scriptable objects is possible by editing this field, but it will not be saved
        this.foodSourceSpecies = this.LevelDataReference.LevelData.FoodSourceSpecies.ToArray();
    }
    private void Update()
    {
        if (this.selectionType == SelectionType.Tile)
        {
            this.TileActions();
        }
        if (this.selectionType == SelectionType.Animal)
        {
            this.AnimalActions();
        }
        if (this.selectionType == SelectionType.FoodSource)
        {
            this.FoodActions();
        }
    }
    private void TileActions()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.tilePlacementController.StartPreview(this.selectedTile.ToString(), this.godMode);
        }
        if (Input.GetMouseButtonUp(0))
        {
            this.tilePlacementController.StopPreview();
        }
        if (Input.GetMouseButtonDown(1))
        {
            this.tilePlacementController.RevertChanges();
        }
    }
    private void AnimalActions()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 location = new Vector3(this.mainCamera.ScreenToWorldPoint(Input.mousePosition).x, this.mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 5);
            this.populationManager.UpdatePopulation(selectedSpecies, location);
        }
        if (Input.GetMouseButtonDown(1))
        {
            this.selectionType = SelectionType.None;
        }
    }
    private void FoodActions()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foodSourceManager.PlaceFood(this.tileSystem.WorldToCell(this.mainCamera.ScreenToWorldPoint(Input.mousePosition)), this.selectedFood);
        }
        if (Input.GetMouseButtonDown(1))
        {
            this.selectionType = SelectionType.None;
        }
    }
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, 200, 600));
        TileSelectionBlock();
        AnimalBlock();
        FoodBlock();
        MapIOBlock();
        GUILayout.EndArea();
        MouseHUD();
        LiquidBlock();
    }
    private void TileSelectionBlock()
    {
        GUILayout.BeginVertical();
        GUILayout.Box("Select with RMB");
        GUILayout.Box("Tiles");
        this.godMode = GUILayout.Toggle(this.godMode, "God Mode");
        this.tileScrollPosition = GUILayout.BeginScrollView(this.tileScrollPosition, GUILayout.Width(200), GUILayout.Height(200));
        foreach (TileType tileType in (TileType[])Enum.GetValues(typeof(TileType)))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(tileType.ToString()))
            {
                this.selectedTile = tileType;
                this.selectionType = SelectionType.Tile;
            }
            if (this.selectedTile == tileType && this.selectionType == SelectionType.Tile)
            {
                GUILayout.Box("Selected");
            }
            GUILayout.EndHorizontal();
        }
        this.tilePlacementController.isErasing = GUILayout.Toggle(this.tilePlacementController.isErasing, "Eraser Mode");
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
    private void MapIOBlock()
    {
        GUILayout.BeginVertical();
        this.sceneName = GUILayout.TextField(this.sceneName);
        if (GUILayout.Button("Save"))
        {
            levelIO.SaveAsPreset(this.sceneName);
        }
        if (GUILayout.Button("Load"))
        {
            foodSourceManager.DestroyAll();
            levelIO.ClearAnimals();
            levelIO.LoadPreset(this.sceneName);
            levelIO.Reload();
        }
        GUILayout.EndVertical();
    }
    private void AnimalBlock()
    {
        GUILayout.Box("Animals");
        this.animalScrollPos = GUILayout.BeginScrollView(this.animalScrollPos);
        foreach (KeyValuePair<string, AnimalSpecies> species in populationManager.speciesReferenceData.AnimalSpecies)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(species.Value.name))
            {
                this.selectedSpecies = species.Value;
                this.selectionType = SelectionType.Animal;
            }
            if (this.selectedSpecies == species.Value && this.selectionType == SelectionType.Animal)
            {
                GUILayout.Box("Selected");
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Clear Animals"))
        {
            this.levelIO.ClearAnimals();
        }
    }
    private void FoodBlock()
    {
        GUILayout.Box("Food Sources");
        this.foodScrollPos = GUILayout.BeginScrollView(this.foodScrollPos);
        foreach (FoodSourceSpecies fs in this.foodSourceSpecies)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(fs.name))
            {
                this.selectedFood = fs;
                this.selectionType = SelectionType.FoodSource;
            }
            if (this.selectedFood == fs && this.selectionType == SelectionType.FoodSource)
            {
                GUILayout.Box("Selected");
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Clear Food"))
        {
            this.foodSourceManager.DestroyAll();
        }
    }
    private void MouseHUD()
    {
        GUILayout.BeginArea(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y -150 , 200, 150));
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        GUILayout.Box("World Pos: " + mousePos.ToString());
        Vector3Int cellPosition = this.tileSystem.WorldToCell(mousePos);
        GUILayout.Box("Cell Pos: " + cellPosition);
        GameTile gameTile = this.tileSystem.GetGameTileAt(cellPosition);
        string name = gameTile ? gameTile.name : "Null";
        LiquidBody liquid = this.tileSystem.GetLiquidBodyAt(cellPosition);
        string bodyID = "Null";
        string con = "Null";
        if (liquid != null)
        {
            bodyID = liquid.bodyID == 0 ? "Preview Body" : liquid.bodyID.ToString();
            con = string.Join(", ", liquid.contents);
        }
        GUILayout.Box("Tile: " + name);
        GUILayout.Box("Liquid Body: " + bodyID);
        GUILayout.Box("Contents: " + con);
        GUILayout.EndArea();
    }
    private void LiquidBlock()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 600));
        GUILayout.BeginVertical();
        GUILayout.Box("Liquid Developer Debugger");
        this.DisplayLiquidBodyInfo = GUILayout.Toggle(this.DisplayLiquidBodyInfo, "Liquid Body Info Display");
        if (this.DisplayLiquidBodyInfo)
        {
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap.TryGetComponent(out TileLayerManager tileLayerManager))
                {
                    if (!tileLayerManager.holdsContent)
                    {
                        continue;
                    }
                    this.LiquidBodyScroll(tileLayerManager, tilemap);
                    this.PreviewBodyScroll(tileLayerManager);
                }
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    private void LiquidBodyScroll(TileLayerManager tileLayerManager, Tilemap tilemap)
    {
        if (!this.ManagersToToggles.ContainsKey(tileLayerManager))
        {
            this.ManagersToToggles.Add(tileLayerManager, new Dictionary<LiquidBody, bool>());
        }
        GUILayout.Box("Tilemap: " + tilemap.name);
        GUILayout.Box("Active Liquid Bodies: " + tileLayerManager.liquidBodies.Count.ToString());
        this.liquidScrollPos = GUILayout.BeginScrollView(liquidScrollPos, GUILayout.Width(200), GUILayout.Height(210));
        foreach (LiquidBody liquidBody in tileLayerManager.liquidBodies)
        {
            if (!this.ManagersToToggles[tileLayerManager].ContainsKey(liquidBody))
            {
                this.ManagersToToggles[tileLayerManager].Add(liquidBody, false);
            }
            GUILayout.Box("LiquidBodyID: " + liquidBody.bodyID);
            GUILayout.Box("Composition");
            for (int i = 0; i < liquidBody.contents.Length; i++)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    liquidBody.contents[i] = float.Parse(GUILayout.TextField(liquidBody.contents[i].ToString("n3")));
                }
                catch { }
                liquidBody.contents[i] = GUILayout.HorizontalSlider(liquidBody.contents[i], 0.0f, 1.0f);
                GUILayout.EndHorizontal();
            }
            GUILayout.Box("Tile Count: " + liquidBody.tiles.Count);
            bool validCrossReference = true;
            foreach (Vector3Int tile in liquidBody.tiles)
            {
                if (tileLayerManager.positionsToTileData[tile].currentLiquidBody != liquidBody)
                {
                    validCrossReference = false;
                    break;
                }
            }
            GUILayout.Box("Referenced Bodies: " + liquidBody.referencedBodies.Count);
            GUILayout.Box("Valid Cross Reference: " + validCrossReference.ToString());
            ManagersToToggles[tileLayerManager][liquidBody] = GUILayout.Toggle(ManagersToToggles[tileLayerManager][liquidBody], "View Area");
            if (ManagersToToggles[tileLayerManager][liquidBody])
            {
                // View Area
            }
        }
        GUILayout.EndScrollView();
    }


    private void PreviewBodyScroll(TileLayerManager tileLayerManager)
    {
        int bodyCount = 0;
        this.previewBodyScrollPos = GUILayout.BeginScrollView(previewBodyScrollPos, GUILayout.Width(200), GUILayout.Height(210));
        GUILayout.Box("Active Preview Bodies: " + tileLayerManager.previewBodies.Count.ToString());
        this.DisplayPreviewBodies = GUILayout.Toggle(this.DisplayPreviewBodies, "Display Preview Bodies");
        foreach (LiquidBody liquidBody in tileLayerManager.previewBodies)
        {
            bodyCount++;
            GUILayout.Box("Body No. " + bodyCount);
            GUILayout.Box("Composition");
            for (int i = 0; i < liquidBody.contents.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box(liquidBody.contents[i].ToString("n3"));
                liquidBody.contents[i] = GUILayout.HorizontalSlider(liquidBody.contents[i], 0.0f, 1.0f);
                GUILayout.EndHorizontal();
            }
            GUILayout.Box("Tile Count: " + liquidBody.tiles.Count);
            bool validCrossReference = true;
            foreach (Vector3Int tile in liquidBody.tiles)
            {
                if (tileLayerManager.positionsToTileData[tile].currentLiquidBody != liquidBody)
                {
                    validCrossReference = false;
                    break;
                }
            }
            GUILayout.Box("Valid Cross Reference" + validCrossReference.ToString());
            if (this.DisplayPreviewBodies)
            {
                // View Area
            }
        }
        GUILayout.EndScrollView();
    }
}
