using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO have this create the food source item and hold it, then have the store display that info
[CreateAssetMenu]
public class FoodSourceSpecies : ScriptableObject
{
    public string SpeciesName => speciesName;
    public int RootRadius => rootRadius;
    public int BaseOutput => baseOutput;
    public Item FoodSourceItem => FoodSource;
    public List<TileType> AccessibleTerrain => accessibleTerrain;
    public int Size => size;

    [SerializeField] private int size = 1; // default to 1 tile big
    [SerializeField] private List<TileType> accessibleTerrain = default;
    [SerializeField] private string speciesName = default;
    [SerializeField] private int rootRadius = default;
    [SerializeField] private int baseOutput = default;
    [SerializeField] private List<NeedTypeConstructData> needsList = default;
    [SerializeField] private Item FoodSource = default;


    public Dictionary<string, Need> SetupNeeds()
    {
        Dictionary<string, Need> needs = new Dictionary<string, Need>();
        foreach (NeedTypeConstructData needData in needsList)
        {
            foreach (NeedConstructData need in needData.Needs)
            {
                // Use the NeedData to create Need
                needs.Add(need.NeedName, new Need(needData.NeedType, need));
                //Debug.Log($"Add {need.NeedName} Need for {this.SpeciesName}");
            }
        }
        return needs;
    }

    public void SetupData(string name, int rootRadius, int output, List<NeedTypeConstructData> needs)
    {
        this.needsList = new List<NeedTypeConstructData>();
        this.speciesName = name;
        this.rootRadius = rootRadius;
        this.baseOutput = output;
        this.needsList = needs;
    }
}
