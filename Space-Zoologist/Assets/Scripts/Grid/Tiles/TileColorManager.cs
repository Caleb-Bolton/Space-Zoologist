﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileColorManager : MonoBehaviour
{
    protected Tilemap tilemap;
    [SerializeField] public ColoringMethod coloringMethod;
    public List<TerrainTile> managedTiles { get { return managedTerrainTiles; } }
    [SerializeField] protected List<TerrainTile> managedTerrainTiles = new List<TerrainTile>();
    public List<TerrainTile> linkedTiles { get { return linkedTerrainTiles; } }
    [SerializeField] protected List<TerrainTile> linkedTerrainTiles = new List<TerrainTile>();
    private TileSystem tileSystem;
    private TilePlacementController tilePlacementController;
    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tileSystem = GetComponentInParent<TileSystem>();
        tilePlacementController = GetComponentInParent<TilePlacementController>();
    }
    public virtual void SetTileColor(float[] composition, Vector3Int cellLocation, TerrainTile tile)
    {
        coloringMethod.SetTileColor(composition, cellLocation, tile, tilemap, managedTerrainTiles, linkedTerrainTiles, tileSystem, tilePlacementController);
    }
}
