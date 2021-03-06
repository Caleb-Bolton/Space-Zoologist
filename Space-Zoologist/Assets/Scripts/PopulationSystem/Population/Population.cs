using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// A runtime instance of a population.
/// </summary>
public class Population : MonoBehaviour, Life
{
    public AnimalSpecies Species { get => species; }
    public int Count { get => this.AnimalPopulation.Count; }
    public float Dominance => Count * species.Dominance;
    public int PrePopulationCount => this.prePopulationCount;
    public Vector3 Origin => this.origin;
    public bool IsPaused => this.isPaused;
    [HideInInspector]
    public bool HasAccessibilityChanged = false;
    public System.Random random = new System.Random();

    public Dictionary<string, Need> Needs => needs;
    public Dictionary<Need, Dictionary<NeedCondition, PopulationBehavior>> NeedBehaviors => needBehaviors;
    public AnimalPathfinding.Grid Grid { get; private set; }
    public List<Vector3Int>  AccessibleLocations { get; private set; }

    public GrowthStatus GrowthStatus => this.GrowthCalculator.GrowthStatus;
    private float animatorSpeed = 0f;
    private float overlaySpeed = 0f;

    [Expandable] public AnimalSpecies species = default;
    [SerializeField] private GameObject AnimalPrefab = default;
    [Header("Add existing animals")]
    [SerializeField] public List<GameObject> AnimalPopulation = default;
    [Header("Lowest Priority Behaviors")]
    [Expandable] public List<PopulationBehavior> DefaultBehaviors = default;
    [Header("Modify values and thresholds for testing")]
    [SerializeField] private List<Need> NeedEditorTesting = default;
    [SerializeField] private List<MovementData> AnimalsMovementData = default;

    private Dictionary<string, Need> needs = new Dictionary<string, Need>();
    private Dictionary<Need, Dictionary<NeedCondition, PopulationBehavior>> needBehaviors = new Dictionary<Need, Dictionary<NeedCondition, PopulationBehavior>>();

    private Vector3 origin = Vector3.zero;
    private GrowthCalculator GrowthCalculator;
    private PoolingSystem PoolingSystem = default;
    private int prePopulationCount = default;
    private PopulationBehaviorManager PopulationBehaviorManager = default;
    private bool isPaused = false;

    private void Awake()
    {
        this.GrowthCalculator = new GrowthCalculator(this);
        this.PopulationBehaviorManager = this.GetComponent<PopulationBehaviorManager>();
        this.PoolingSystem = this.GetComponent<PoolingSystem>();
        if (this.species != null)
        {
            this.SetupNeeds();
            this.origin = this.transform.position;
        }
    }

    private void Start()
    {
        int i=0;
        foreach(PopulationBehavior behaviorPattern in this.DefaultBehaviors)
        {
            this.PopulationBehaviorManager.ActiveBehaviors.Add("default" + i, behaviorPattern);
            i++;
        }
    }

    /// <summary>
    /// Initialize the population as the given species at the given origin after runtime.
    /// </summary>
    /// <param name="species">The species of the population</param>
    /// <param name="origin">The origin of the population</param>
    /// <param name="needSystemManager"></param>
    ///  TODO population instantiation should likely come from an populationdata object with more fields
    public void InitializeNewPopulation(AnimalSpecies species, Vector3 origin, int populationSize)
    {
        this.species = species;
        this.origin = origin;
        this.transform.position = origin;

        this.PoolingSystem.AddPooledObjects(5, this.AnimalPrefab);
        for (int i = 0; i < populationSize; i++)
        {
            GameObject newAnimal = Instantiate(this.AnimalPrefab, this.transform);
            this.AnimalPopulation.Add(newAnimal);
            // PopulationManager will explicitly initialize a new population's animal at the very end
            this.AnimalPopulation[i].SetActive(true);
        }
        this.SetupNeeds();
    }

    private void SetupNeeds()
    {
        this.needs = this.Species.SetupNeeds();
        this.needBehaviors = this.Species.SetupBehaviors(this.needs);
        //this.PopulationBehaviorManager.isPaused = true;
        this.PopulationBehaviorManager.InitializeBehaviors(this.needs);
        this.NeedEditorTesting = new List<Need>();
        foreach (KeyValuePair<string, Need> need in this.needs)
        {
            this.NeedEditorTesting.Add(need.Value);
            this.GrowthCalculator.setupNeedTimer(need.Key, need.Value.Severity);
        }
    }

    /// <summary>
    /// Grabs the updated accessible area and then resets the behavior for all of the animals.
    /// </summary>
    /// Could improve by checking what shape the accessible area is in
    public void UpdateAccessibleArea(List<Vector3Int> accessibleLocations, AnimalPathfinding.Grid grid)
    {
        this.AccessibleLocations = accessibleLocations;
        this.Grid = grid;
        foreach(GameObject animal in this.AnimalPopulation)
        {
            if (!this.AccessibleLocations.Contains(this.Grid.grid.WorldToCell(animal.transform.position)))
            {
                animal.transform.position = this.origin;
            }
        }
    }

    // Only pauses movements
    public void PauseAnimalsMovementController()
    {
        foreach(GameObject animal in this.AnimalPopulation)
        {
            this.isPaused = true;
            Animator animator = animal.GetComponent<Animator>();
            Animator overlay = animal.transform.GetChild(0).GetComponent<Animator>();
            if (animator.speed != 0)
            {
                this.animatorSpeed = animator.speed;
            }
            if (overlay.speed != 0)
            {
                this.overlaySpeed = overlay.speed;
            }
            animator.speed = 0;
            overlay.speed = 0;
            animal.GetComponent<MovementController>().IsPaused = true;
            //animal.GetComponent<MovementController>().TryToCancelDestination();
        }
    }

    public void UnpauseAnimalsMovementController()
    {
        foreach(GameObject animal in this.AnimalPopulation)
        {
            this.isPaused = false;
            Animator animator = animal.GetComponent<Animator>();
            Animator overlay = animal.transform.GetChild(0).GetComponent<Animator>();
            animator.speed = this.animatorSpeed;
            overlay.speed = this.overlaySpeed;
            animal.GetComponent<MovementController>().IsPaused = false;
        }
    }

    public void InitializeExistingAnimals()
    {
        foreach (GameObject animal in this.AnimalPopulation)
        {
            if (animal.activeSelf)
            {
                MovementData data = new MovementData();
                this.AnimalsMovementData.Add(data);
                animal.GetComponent<Animal>().Initialize(this, data);
            }
        }
        this.prePopulationCount = this.AnimalPopulation.Count;
        this.PopulationBehaviorManager.Initialize();
    }

    /// <summary>
    /// Update the given need of the population with the given value.
    /// </summary>
    /// <param name="need">The need to update</param>
    /// <param name="value">The need's new value</param>
    public void UpdateNeed(string need, float value)
    {
        Debug.Assert(this.needs.ContainsKey(need), $"{ species.SpeciesName } population has no need { need }");
        this.needs[need].UpdateNeedValue(value);
        // Debug.Log(need + " now has a value of " + this.needs[need].NeedValue + " and is now in a " + this.needs[need].GetCondition(value) + " condition");
        this.FilterBehaviors(need, this.needs[need].GetCondition(value));
        // Debug.Log($"The { species.SpeciesName } population { need } need has new value: {this.needs[need].NeedValue}");
    }

    // TODO figure out filter bug for behaviors
    /// <summary>
    /// Updates the needs behaviors based on the need's current condition
    /// </summary>
    /// Currently filtering behaviors using null, may want to change.
    public void FilterBehaviors(string need, NeedCondition needCondition)
    {
        if (this.PopulationBehaviorManager.ActiveBehaviors.ContainsKey(need))
        {
            // previous implementation
            //this.PopulationBehaviorManager.ActiveBehaviors[need] = this.needBehaviors[this.needs[need]][needCondition];

            this.PopulationBehaviorManager.ActiveBehaviors[need] = this.needs[need].GetBehavior(needs[need].NeedValue).Behavior;
        }
    }

    /// <summary>
    /// Get the value of the given need.
    /// </summary>
    /// <param name="need">The need to get the value of</param>
    /// <returns></returns>
    public float GetNeedValue(string need)
    {
        Debug.Assert(this.needs.ContainsKey(need), $"{ species.SpeciesName } population has no need { need }");
        return this.needs[need].NeedValue;
    }

    // Add one because UpdateGrowthConditions updates this value independently of HandleGrowth
    public int DaysTillDeath(String need)
    {
        return this.GrowthCalculator.needTimers[need] + 1;
    }

    // Don't add one because this value is updated when HandleGrowth is called
    public int DaysTillGrowth()
    {
        return this.GrowthCalculator.GrowthCountdown;
    }

    /// <summary>
    /// Calculate growth, then remove or add animals as needed.
    /// </summary>
    public void UpdateGrowthConditions()
    {
        if (this.Species == null) return;

        this.GrowthCalculator.CalculateGrowth();
    }

    public void HandleGrowth()
    {
        switch (this.GrowthCalculator.GrowthStatus)
        {
            case GrowthStatus.growing:
                if (this.GrowthCalculator.ReadyForGrowth())
                {
                    this.AddAnimal();
                }
                break;
            case GrowthStatus.declining:
                for (int i=0; i<this.GrowthCalculator.NumAnimalsToRemove(); i++)
                {
                    this.RemoveAnimal();
                }
                break;
            default:
                break;
        }
    }

    public void AddAnimal()
    {
        MovementData data = new MovementData();
        this.AnimalsMovementData.Add(data);
        GameObject newAnimal = this.PoolingSystem.GetPooledObject(this.AnimalPopulation);
        if (newAnimal == null)
        {
            this.PoolingSystem.AddPooledObjects(5, this.AnimalPrefab);
            newAnimal = this.PoolingSystem.GetPooledObject(this.AnimalPopulation);
        }
        newAnimal.GetComponent<Animal>().Initialize(this, data);
        this.PopulationBehaviorManager.animalsToExecutionData.Add(newAnimal, new BehaviorExecutionData(0));
        this.PopulationBehaviorManager.OnBehaviorComplete(newAnimal);
        // Invoke a population growth event
        EventManager.Instance.InvokeEvent(EventType.PopulationCountIncreased, this);
    }

    // removes last animal in list and last behavior
    // TODO remove count of animals
    public void RemoveAnimal()
    {
        if (this.AnimalPopulation.Count == 0)
        {
            Debug.Log(this.gameObject.name + " population already exitinct");
            return;
        }
        if (this.AnimalPopulation.Count > 0)
        {
            Debug.Log("Animal removed");
            this.AnimalsMovementData.RemoveAt(this.AnimalsMovementData.Count - 1);
            this.PopulationBehaviorManager.RemoveAnimal(this.AnimalPopulation[this.AnimalPopulation.Count - 1]);
            this.AnimalPopulation[this.AnimalPopulation.Count - 1].SetActive(false);
            this.PoolingSystem.ReturnObjectToPool(this.AnimalPopulation[this.AnimalPopulation.Count - 1]);
            this.AnimalPopulation.RemoveAt(this.AnimalPopulation.Count - 1);
            if (this.AnimalPopulation.Count == 0)
            {
                Debug.Log("Population " + this.gameObject.name + " has gone extinct!");
                // TODO Delete the population at another time, or else the reference will be lost
                EventManager.Instance.InvokeEvent(EventType.PopulationExtinct, this);
            }
            else
            {
                // Invoke a population decline event
                EventManager.Instance.InvokeEvent(EventType.PopulationCountDecreased, this);
            }
        }
    }

    // Ensure there are enough behavior data scripts mapped to the population size
    void OnValidate()
    {
        // while (this.AnimalsMovementData.Count < this.AnimalPopulation.Count)
        // {
        //     this.AnimalsMovementData.Add(new MovementData());
        // }
        // while (this.AnimalsMovementData.Count > this.AnimalPopulation.Count)
        // {
        //     this.AnimalsMovementData.RemoveAt(this.AnimalsMovementData.Count - 1);
        // }
        //if (this.GrowthCalculator != null)
        //{
        //    this.UpdateEditorNeeds();
        //    this.UpdateGrowthConditions();
        //}
    }

    private void UpdateEditorNeeds()
    {
        // Debug.Log("Needs updated with editor");
        int i=0;
        foreach (KeyValuePair<string, Need> need in this.needs)
        {
            if (this.NeedEditorTesting[i].NeedName.Equals(need.Key))
            {
                this.NeedEditorTesting[i] = need.Value;
            }
            i++;
        }
        if (this.NeedEditorTesting != null)
        {
            foreach (Need need in this.NeedEditorTesting)
            {
                this.needs[need.NeedName] = need;
            }
        }
    }

    public Dictionary<string, Need> GetNeedValues()
    {
        return this.Needs;
    }

    public Vector3 GetPosition()
    {
        return this.gameObject.transform.position;
    }

    public bool GetAccessibilityStatus()
    {
        return this.HasAccessibilityChanged;
    }

    public void UpdatePopulationStateForChecking()
    {
        this.HasAccessibilityChanged = false;
        this.prePopulationCount = this.Count;
    }
}
