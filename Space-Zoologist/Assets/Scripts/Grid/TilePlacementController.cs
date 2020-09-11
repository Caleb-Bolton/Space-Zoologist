using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacementController : MonoBehaviour
{
    public bool isBlockMode { get; set; } = false;
    public Vector3Int mouseCellPosition { get { return currentMouseCellPosition; } }
    public bool PlacementPaused { get; private set; }
    [SerializeField] private Camera currentCamera = default;
    private bool isPreviewing { get; set; } = false;
    private Vector3Int dragStartPosition = Vector3Int.zero;
    private Vector3Int lastMouseCellPosition = Vector3Int.zero;
    private Vector3Int currentMouseCellPosition = Vector3Int.zero;
    private Grid grid;
    private Vector3Int lastPlacedTile;
    private List<TerrainTile> referencedTiles = new List<TerrainTile>();
    private bool isFirstTile;
    public Tilemap[] allTilemaps { get { return tilemaps; } }
    [SerializeField] private Tilemap[] tilemaps = default; // Order according to GridUtils.TileLayer
    private TerrainTile[] terrainTiles = default;
    private Dictionary<Vector3Int, List<TerrainTile>> addedTiles = new Dictionary<Vector3Int, List<TerrainTile>>(); // All NEW tiles
    private Dictionary<Vector3Int, List<TerrainTile>> removedTiles = new Dictionary<Vector3Int, List<TerrainTile>>(); //All tiles removed
    private Dictionary<Vector3Int, Dictionary<Color, Tilemap>> removedTileColors = new Dictionary<Vector3Int, Dictionary<Color, Tilemap>>();
    private HashSet<Vector3Int> triedToPlaceTiles = new HashSet<Vector3Int>(); // New tiles and same tile
    private HashSet<Vector3Int> neighborTiles = new HashSet<Vector3Int>();
    private Dictionary<TerrainTile, List<Tilemap>> colorLinkedTiles = new Dictionary<TerrainTile, List<Tilemap>>();
    private int lastCornerX;
    private int lastCornerY;
    [SerializeField] private TileSystem TileSystem = default;
    [SerializeField] private GridSystem GridSystem = default;
    private void Awake()
    {
        terrainTiles = Resources.LoadAll("Tiles",typeof(TerrainTile)).Cast<TerrainTile>().ToArray(); // Load tiles form resources
        foreach (TerrainTile terrainTile in terrainTiles)// Construct list of tiles and their corresponding layers
        {
            terrainTile.targetTilemap = tilemaps[(int)terrainTile.targetLayer];
            terrainTile.constraintTilemap.Clear();
            terrainTile.replacementTilemap.Clear();
            terrainTile.ReferencePlaceableArea(); //Add PlaceableArea Tilemap reference to all tiles
            foreach (GridUtils.TileLayer layer in terrainTile.constraintLayers)
            {
                terrainTile.constraintTilemap.Add(tilemaps[(int)layer]);
            }
            foreach (GridUtils.TileLayer layer in terrainTile.replacementLayers)
            {
                terrainTile.replacementTilemap.Add(tilemaps[(int)layer]);
            }
        }
    }

    private void Start()
    {
        grid = GetComponent<Grid>();
        foreach (Tilemap tilemap in tilemaps)// Construct list of affected colors
        {
            List<Vector3Int> colorInitializeTiles = new List<Vector3Int>();
            if (tilemap.TryGetComponent(out TileColorManager tileColorManager))
            {
                foreach (TerrainTile tile in tileColorManager.linkedTiles)
                {
                    if (!colorLinkedTiles.ContainsKey(tile))
                    {
                        colorLinkedTiles.Add(tile, new List<Tilemap>());
                    }
                    colorLinkedTiles[tile].Add(tilemap);
                }
                foreach(Vector3Int cellLocation in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(cellLocation))
                    {
                        if (!colorInitializeTiles.Contains(cellLocation))
                        {
                            colorInitializeTiles.Add(cellLocation);
                        }
                    }
                }
            }
            referencedTiles = terrainTiles.ToList();
            RenderColorOfColorLinkedTiles(colorInitializeTiles);
            referencedTiles.Clear();
        }
        
    }

    void Update()
    {
        if (isPreviewing) // Update for preview
        {
            Vector3 mouseWorldPosition = currentCamera.ScreenToWorldPoint(Input.mousePosition);
            currentMouseCellPosition = grid.WorldToCell(mouseWorldPosition);
            this.PlacementPaused = false;
            if (currentMouseCellPosition != lastMouseCellPosition || isFirstTile)
            {
                if (isBlockMode)
                {
                    UpdatePreviewBlock();
                }
                else
                {
                    UpdatePreviewPen();
                }
                lastMouseCellPosition = currentMouseCellPosition;
            }
        }
    }

    /// <summary>
    /// Start tile placement preview.
    /// </summary>
    /// <param name="tileID">The ID of the tile to preview its placement.</param>
    public void StartPreview(string tileID)
    {
        Vector3 mouseWorldPosition = currentCamera.ScreenToWorldPoint(Input.mousePosition);
        dragStartPosition = grid.WorldToCell(mouseWorldPosition);
        if (!Enum.IsDefined(typeof(TileType), tileID))
        {
            throw new System.ArgumentException(tileID + " was not found in the TilePlacementController's tiles");
        }
        isPreviewing = true;
        foreach (TerrainTile tile in terrainTiles)
        {
            if (tile.type == (TileType)Enum.Parse(typeof(TileType), tileID))
            {
                referencedTiles.Add(tile);
            }
        }
        isFirstTile = true;
    }

    public void StopPreview()
    {
        isPreviewing = false;
        lastMouseCellPosition = Vector3Int.zero;
        foreach (Tilemap tilemap1 in tilemaps)
        {
            if (tilemap1.TryGetComponent(out TileContentsManager tilecContentsManager))
            {
                tilecContentsManager.ConfirmMerge();
            }
        }
        RenderColorOfColorLinkedTiles(addedTiles.Keys.ToList());
        foreach (TerrainTile tile in referencedTiles)
        {
            if (tile.targetTilemap.GetComponent<TileContentsManager>() == null && tile.targetTilemap.TryGetComponent(out TileColorManager placedTileColorManager))
            {
                foreach (Vector3Int vector3Int in addedTiles.Keys)
                {
                    placedTileColorManager.SetTileColor(vector3Int, tile);
                }
            }
        }

        // Set terrain modified flag
        this.TileSystem.HasTerrainChanged = true;
        this.TileSystem.chagnedTiles.AddRange(addedTiles.Keys.ToList());

        // Invoke event and pass the changed tiles that are not walls
        EventManager.Instance.InvokeEvent(EventType.TerrainChange, this.TileSystem.chagnedTiles.FindAll(
            pos => this.TileSystem.GetTerrainTileAtLocation(pos).type != TileType.Wall
        ));

        // Clear all dics
        referencedTiles.Clear();
        removedTileColors.Clear();
        addedTiles.Clear();
        removedTiles.Clear();
        triedToPlaceTiles.Clear();
    }

    public int PlacedTileCount()
    {
        return addedTiles.Count();
    }

    public void RevertChanges() // Go through each change and revert back to original
    {
        foreach (Vector3Int changedTileLocation in triedToPlaceTiles)
        {
            if (addedTiles.ContainsKey(changedTileLocation))
            {
                foreach (TerrainTile addedTile in addedTiles[changedTileLocation])
                {
                    addedTile.targetTilemap.SetTile(changedTileLocation, null);
                }
            }
            if (removedTiles.ContainsKey(changedTileLocation))
            {
                foreach (TerrainTile removedTile in removedTiles[changedTileLocation])
                {
                    removedTile.targetTilemap.SetTile(changedTileLocation, removedTile);
                }
            }
        }
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.TryGetComponent(out TileContentsManager tileAttributes))
            {
                List<Vector3Int> changedTiles = tileAttributes.changedTilesPositions;
                changedTiles.AddRange(tileAttributes.addedTilePositions);
                tileAttributes.Revert();
                RenderColorOfColorLinkedTiles(changedTiles);
            }
        }
        foreach (Vector3Int colorChangedTiles in removedTileColors.Keys)
        {
            removedTileColors[colorChangedTiles].Values.First().SetColor(colorChangedTiles, removedTileColors[colorChangedTiles].Keys.First());
        }
        removedTileColors.Clear();
        addedTiles.Clear();
        removedTiles.Clear();
        triedToPlaceTiles.Clear();
        StopPreview();
    }

    public void RenderColorOfColorLinkedTiles(List<Vector3Int> changedTiles) // Update color for linked tiles.
    {
        foreach (TerrainTile tile in referencedTiles)
        {
            if (colorLinkedTiles.Keys.Contains(tile))
            {
                foreach (Tilemap tilemap in colorLinkedTiles[tile])
                {
                    TileColorManager tileColorManager = tilemap.GetComponent<TileColorManager>();
                    foreach (Vector3Int addedTileLocation in changedTiles)
                    {
                        foreach (TerrainTile managedTile in tileColorManager.managedTiles)
                        {
                            foreach (Vector3Int affectedTileLocation in this.TileSystem.AllCellLocationsOfTileInRange(addedTileLocation, tileColorManager.coloringMethod.affectedRange, managedTile))
                            {
                                tileColorManager.SetTileColor(affectedTileLocation, managedTile);
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdatePreviewPen()
    {
        if (isFirstTile)
        {
            PlaceTile(currentMouseCellPosition);
            return;
        }
        if (!GridUtils.FourNeighborTiles(currentMouseCellPosition).Contains(lastPlacedTile)) // Detect non-continuous points, and linearly interpolate to fill gaps
        {
            if (currentMouseCellPosition.x == lastPlacedTile.x)// Handles divide by zero exception
            {
                foreach (int y in GridUtils.Range(lastPlacedTile.y, currentMouseCellPosition.y))
                {
                    Vector3Int location = new Vector3Int(lastPlacedTile.x, y, currentMouseCellPosition.z);
                    PlaceTile(location);
                }
            }
            else
            {
                float gradient = (currentMouseCellPosition.y - lastPlacedTile.y) / (currentMouseCellPosition.x - lastPlacedTile.x);
                foreach (float x in GridUtils.RangeFloat(GridUtils.IncreaseMagnitude(lastPlacedTile.x, -0.5f), currentMouseCellPosition.x))
                {
                    float interpolatedY = gradient * (x - lastPlacedTile.x);
                    int incrementY = GridUtils.RoundTowardsZeroInt(interpolatedY);
                    Vector3Int interpolateTileLocation = new Vector3Int(GridUtils.RoundTowardsZeroInt(x), lastPlacedTile.y + incrementY, lastPlacedTile.z);
                    PlaceTile(interpolateTileLocation);
                }
            }
        }
        PlaceTile(currentMouseCellPosition);
    }

    private void UpdatePreviewBlock()
    {
        if (isFirstTile)
        {
            PlaceTile(dragStartPosition, false);
            lastCornerX = dragStartPosition.x;
            lastCornerY = dragStartPosition.y;
        }
        List<Vector3Int> tilesToRemove = new List<Vector3Int>();
        List<Vector3Int> tilesToAdd = new List<Vector3Int>();
        List<Vector3Int> supposedTiles = new List<Vector3Int>();
        foreach (int x in GridUtils.Range(dragStartPosition.x, currentMouseCellPosition.x))
        {
            foreach (int y in GridUtils.Range(dragStartPosition.y, currentMouseCellPosition.y))
            {
                supposedTiles.Add(new Vector3Int(x, y, currentMouseCellPosition.z));
            }
        }
        foreach (Vector3Int existingTile in addedTiles.Keys) // Forcing removal of all tiles not in bound to avoid leftover tile not being removed due to lagging and tick skipping, possible optimization
        {
            if (!supposedTiles.Contains(existingTile))
            {
                tilesToRemove.Add(existingTile);
            }
        }
        Vector3Int sweepLocation = Vector3Int.zero;
        sweepLocation.z = currentMouseCellPosition.z;
        bool isXShrinking = GridUtils.IsOppositeSign(currentMouseCellPosition.x - dragStartPosition.x, currentMouseCellPosition.x - lastCornerX);
        bool isYShrinking = GridUtils.IsOppositeSign(currentMouseCellPosition.y - dragStartPosition.y, currentMouseCellPosition.y - lastCornerY);
        if (currentMouseCellPosition.x != lastCornerX || !isXShrinking)
        {
            foreach (int x in GridUtils.Range(lastCornerX, currentMouseCellPosition.x))
            {
                foreach (int y in GridUtils.Range(dragStartPosition.y, currentMouseCellPosition.y))
                {
                    sweepLocation.x = x;
                    sweepLocation.y = y;
                    tilesToAdd.Add(sweepLocation);
                }
            }
        }
        if (currentMouseCellPosition.y != lastCornerY || !isYShrinking)
        {
            foreach (int x in GridUtils.Range(dragStartPosition.x, currentMouseCellPosition.x))
            {
                foreach (int y in GridUtils.Range(lastCornerY, currentMouseCellPosition.y))
                {
                    sweepLocation.x = x;
                    sweepLocation.y = y;
                    if (!tilesToRemove.Contains(sweepLocation) && !tilesToAdd.Contains(sweepLocation))
                    {
                        tilesToAdd.Add(sweepLocation);
                    }
                }
            }
        }
        foreach (Vector3Int addLocation in tilesToAdd)
        {
            PlaceTile(addLocation);
        }
        if (tilesToRemove.Count > 0)
        {
            foreach (Vector3Int removeLocation in tilesToRemove)
            {
                RestoreReplacedTile(removeLocation);
            }
            foreach (TerrainTile tile in referencedTiles)
            {
                if (tile.targetTilemap.TryGetComponent(out TileContentsManager tileAttributes))
                {
                    tileAttributes.Revert(supposedTiles);
                }
            }

        }
        lastCornerX = currentMouseCellPosition.x;
        lastCornerY = currentMouseCellPosition.y;
    }

    private bool IsPlacable(Vector3Int cellLocation)
    {

        if (currentMouseCellPosition == dragStartPosition)
        {
            return true;
        }
        foreach (Vector3Int location in GridUtils.FourNeighborTiles(cellLocation))
        {
            if (triedToPlaceTiles.Contains(location))
            {
                return true;
            }
        }
        return false;
    }

    private bool PlaceTile(Vector3Int cellLocation, bool checkPlacable = true) //Main function controls tile placement
    {
        if (IsPlacable(cellLocation) || !checkPlacable)
        {
            foreach (TerrainTile tile in referencedTiles)
            {
                // Remove conflicting tiles
/*                foreach (Tilemap replacingTilemap in tile.replacementTilemap)
                {
                    if (replacingTilemap.HasTile(cellLocation))
                    {
                        ReplaceTile(replacingTilemap, cellLocation);
                    }
                }*/
                // Add new tiles
                TerrainTile replacedTile = (TerrainTile)tile.targetTilemap.GetTile(cellLocation);
                if (tile != replacedTile)
                {
                    if (tile.constraintTilemap.Count > 0)
                    {
                        foreach (Tilemap constraintTilemap in tile.constraintTilemap)
                        {
                            if (!constraintTilemap.HasTile(cellLocation) || !IsTileFree(cellLocation))
                            {
                                break;
                            }
                            foreach (Tilemap replacingTilemap in tile.replacementTilemap)
                            {
                                if (replacingTilemap.HasTile(cellLocation))
                                {
                                    ReplaceTile(replacingTilemap, cellLocation);
                                }
                            }
                            if (replacedTile != null)
                            {
                                ReplaceTile(tile.targetTilemap, cellLocation);
                            }
                            AddNewTile(cellLocation, tile);
                            break;
                        }

                    }
                    else
                    {
                        if (replacedTile != null)
                        {
                            ReplaceTile(replacedTile.targetTilemap, cellLocation);
                        }
                        AddNewTile(cellLocation, tile);
                    }
                }
                else
                {
                    triedToPlaceTiles.Add(cellLocation);
                }
                lastPlacedTile = cellLocation;
                isFirstTile = false;
            }

            // Terrain changed, mark TerrainNS dirty
            //NeedSystemManager.ins.Systems[NeedType.Terrain].MarkAsDirty();

            return true;
        }
        else
        {
            return false;
        }
    }

    private void RestoreReplacedTile (Vector3Int cellLocation)
    {
        foreach (TerrainTile addedTile in addedTiles[cellLocation])
        {
            addedTile.targetTilemap.SetTile(cellLocation, null);
            if (removedTiles.ContainsKey(cellLocation))
            {
                foreach (TerrainTile removedTile in removedTiles[cellLocation])
                {
                    removedTile.targetTilemap.SetTile(cellLocation, removedTile);
                    if (removedTile.targetTilemap.TryGetComponent(out TileContentsManager tileAttributes))
                    {
                        tileAttributes.Restore(cellLocation);
                    }
                }
            }
            addedTiles.Remove(cellLocation);
        }
        if (removedTileColors.ContainsKey(cellLocation))
        {
            removedTileColors[cellLocation].Values.First().SetColor(cellLocation, removedTileColors[cellLocation].Keys.First());
            removedTileColors.Remove(cellLocation);
        }
    }

    private void AddNewTile(Vector3Int cellLocation, TerrainTile tile)
    {
        triedToPlaceTiles.Add(cellLocation);
        if (!addedTiles.ContainsKey(cellLocation))
        {
            addedTiles.Add(cellLocation, new List<TerrainTile> { tile });
        }
        else
        {
            addedTiles[cellLocation].Add(tile);
        }
        tile.targetTilemap.SetTile(cellLocation, tile);
        if (tile.targetTilemap.TryGetComponent(out TileContentsManager tileAttributes))
        {
            tileAttributes.MergeTile(cellLocation, tile, addedTiles.Keys.ToList());
        }
    }

    private void ReplaceTile(Tilemap replacedTilemap, Vector3Int cellLocation)
    {
        TerrainTile removedTile = (TerrainTile)replacedTilemap.GetTile(cellLocation);
        if (!removedTiles.ContainsKey(cellLocation))
        {
            removedTiles.Add(cellLocation, new List<TerrainTile> { removedTile });
        }
        else if (!removedTiles[cellLocation].Contains(removedTile))
        {
            removedTiles[cellLocation].Add(removedTile);
        }
        if (replacedTilemap.TryGetComponent(out TileContentsManager tileAttributes))
        {
            tileAttributes.RemoveTile(cellLocation);
        }
        if (replacedTilemap.TryGetComponent(out TileColorManager tileColorManager))
        {
            if (!removedTileColors.ContainsKey(cellLocation))
            {
                removedTileColors.Add(cellLocation, new Dictionary<Color, Tilemap>());
                removedTileColors[cellLocation].Add(replacedTilemap.GetColor(cellLocation), replacedTilemap);
            }
        }
        replacedTilemap.SetTile(cellLocation, null);
    }
    private void GetNeighborCellLocations(Vector3Int cellLocation, TerrainTile tile, Tilemap targetTilemap)
    {
        foreach (Vector3Int tileToCheck in GridUtils.FourNeighborTiles(cellLocation))
        {
            if (!neighborTiles.Contains(tileToCheck) && targetTilemap.GetTile(tileToCheck) == tile)
            {
                neighborTiles.Add(tileToCheck);
                GetNeighborCellLocations(tileToCheck, tile, targetTilemap);
            }
        }
    }
    private bool IsTileFree(Vector3Int cellLocation)
    {
        GridSystem.CellData cellData = GridSystem.CellGrid[cellLocation[0], cellLocation[1]];
        return (!cellData.ContainsAnimal && !cellData.ContainsFood && !cellData.ContainsMachine && !cellData.HomeLocation);
    }
}
