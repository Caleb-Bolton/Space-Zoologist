using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A runtime instance of a food source
/// </summary>
public class FoodSource : MonoBehaviour, Life
{
    public FoodSourceSpecies Species => species;
    public float FoodOutput => CalculateOutput();
    public Vector2 Position { get; private set; } = Vector2.zero;

    public Dictionary<string, Need> Needs => needs;
    private Dictionary<string, Need> needs = new Dictionary<string, Need>();

    // For runtime instances of a food source
    [Expandable][SerializeField] private FoodSourceSpecies species = default;
    [SerializeField] private TileSystem tileSystem = default;

    private float neutralMultiplier = 0.5f;
    private float goodMultiplier = 1.0f;

    private int[] accessibleTerrian = new int[(int)TileType.TypesOfTiles];
    private bool hasAccessibilityChanged = default;
    private bool hasAccessibilityChecked = default;

    private TileSystem TileSystem = default;

    // To figure out if the output has changed, in order to invoke vent
    private float prevOutput = 0;

    private void Awake()
    {
        if (species)
        {
            InitializeFoodSource(species, transform.position, this.tileSystem);
        }
    }

    // Subscribe to events here
    private void Start()
    {
        // If the food has atmospheric need then subscribe to atmosphere changed event
        foreach (AtmosphereComponent atmosphereComponent in Enum.GetValues(typeof(AtmosphereComponent)))
        {
            if (this.needs.ContainsKey(atmosphereComponent.ToString()))
            {
                EventManager.Instance.SubscribeToEvent(EventType.AtmosphereChange, () =>
                {
                    this.hasAccessibilityChecked = true;
                    this.hasAccessibilityChanged = true;
                });
            }
        }
    }

    public void InitializeFoodSource(FoodSourceSpecies species, Vector2 position, TileSystem tileSystem)
    {
        this.species = species;
        this.Position = position;
        this.GetComponent<SpriteRenderer>().sprite = species.FoodSourceItem.Icon;
        this.InitializeNeedValues();
        this.TileSystem = tileSystem;
        this.accessibleTerrian = this.TileSystem.CountOfTilesInRange(Vector3Int.FloorToInt(this.Position), this.Species.RootRadius);
    }

    private void InitializeNeedValues()
    {
        this.needs = this.species.SetupNeeds();
    }

    private float CalculateOutput()
    {
        foreach (KeyValuePair<string, Need> needValuePair in this.needs)
        {
            string needType = needValuePair.Key;
            float needValue = needValuePair.Value.NeedValue;
            if (!needIsSatisified(needType, needValue))
            {
                updatePreviousOutput(0);
                return 0;
            }
            // output += calculateNeedOutput(needType, needValue, severityTotal);
        }

        updatePreviousOutput(species.BaseOutput);
        return species.BaseOutput;
    }

    private bool needIsSatisified(string needType, float needValue)
    {
        NeedCondition condition = this.needs[needType].GetCondition(needValue);
        // A need is not satisfied
        if (condition != NeedCondition.Good)
        {
            return false;
        }
        return true;
    }

    // Variable output currently removed from design
    private float calculateNeedOutput(string needType, float needValue, float severityTotal)
    {
        NeedCondition condition = this.needs[needType].GetCondition(needValue);
        float multiplier = 0;
        switch (condition)
        {
            case NeedCondition.Bad:
                multiplier = 0;
                break;
            case NeedCondition.Neutral:
                multiplier = neutralMultiplier;
                break;
            case NeedCondition.Good:
                multiplier = goodMultiplier;
                break;
        }
        float needSeverity = this.needs[needType].Severity;
        float output = multiplier * (needSeverity / severityTotal) * species.BaseOutput;
        return output;
    }

    private void updatePreviousOutput(float output)
    {
        // Invoke event if output is different
        if (this.prevOutput != 0 && this.prevOutput != output)
        {
            EventManager.Instance.InvokeEvent(EventType.FoodSourceOutputChange, this);
        }

        this.prevOutput = output;
    }

    /// <summary>
    /// Update the given need of the population with the given value.
    /// </summary>
    /// <param name="need">The need to update</param>Z
    /// <param name="value">The need's new value</param>
    public void UpdateNeed(string need, float value)
    {
        Debug.Assert(this.needs.ContainsKey(need), $"{ species.SpeciesName } food source has no need { need }");
        this.needs[need].UpdateNeedValue(value);
        // Debug.Log($"The { species.SpeciesName } population { need } need has new value: {NeedsValues[need]}");
    }

    public Dictionary<string, Need> GetNeedValues()
    {
        return this.Needs;
    }

    public Vector3 GetPosition()
    {
        return this.gameObject.transform.position;
    }

    /// <summary>
    /// Checks accessible terrain info, ie terrain tile composition
    /// </summary>
    /// <remarks>
    /// Actual checking will only be done once per universal NS update loop,
    /// since terrain will not change during that time
    /// </remarks>
    /// <returns>True is accessible terrain had changed, false otherwise</returns>
    public bool GetAccessibilityStatus()
    {
        // No need to check if terrain was not modified
        if (!this.TileSystem.HasTerrainChanged)
        {
            return false;
        }

        // Return result if have checked
        if (this.hasAccessibilityChecked)
        {
            return this.hasAccessibilityChanged;
        }

        var preTerrain = this.accessibleTerrian;
        var curTerrain = this.TileSystem.CountOfTilesInRange(Vector3Int.FloorToInt(this.Position), this.Species.RootRadius);

        // Accessible terrain had changed
        this.hasAccessibilityChecked = true;
        if(!preTerrain.SequenceEqual(curTerrain))
        {
            this.hasAccessibilityChanged = true;
        }

        return this.hasAccessibilityChanged;
    }

    /// <summary>
    /// Updates the accessible terrain info
    /// </summary>
    public void UpdateAccessibleTerrainInfo()
    {
        if (this.hasAccessibilityChanged)
        {
            this.accessibleTerrian = this.TileSystem.CountOfTilesInRange(Vector3Int.FloorToInt(this.Position), this.Species.RootRadius);
        }

        // Reset flags
        this.hasAccessibilityChecked = false;
        this.hasAccessibilityChanged = false;
    }
}
