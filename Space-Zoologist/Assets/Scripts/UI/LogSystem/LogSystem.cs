﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This system hanldes creating, saving and displaying the logs. 
/// </summary>
public class LogSystem : MonoBehaviour
{
    /// <summary>
    /// Data structure to store info of a log entry
    /// </summary>
    private class LogEntry
    {
        private string logTime;
        private string logText;

        public LogEntry(string logTime, string logText)
        {
            this.logTime = logTime;
            this.logText = logText;
        }

        public string GetDisplay()
        {
            return $"[{this.logTime}] {this.logText}";
        }
    }

    // Stores all logs
    private List<LogEntry> worldLog = default;
    // Stores logs about populations
    private Dictionary<Population, List<LogEntry>> populationLogs = default;
    // Stores logs about food source
    private Dictionary<FoodSource, List<LogEntry>> foodSourceLogs = default;
    // Stores logs about enclosed area
    private Dictionary<EnclosedArea, List<LogEntry>> enclosedAreaLogs = default;

    private bool isInLogSystem = false;

    // Log window
    [SerializeField] private GameObject logWindow = default;
    // Log text
    [SerializeField] private Text logWindowText = default;

    private EventManager eventManager;

    private void Awake()
    {
        this.worldLog = new List<LogEntry>();
        this.populationLogs = new Dictionary<Population, List<LogEntry>>();
        this.foodSourceLogs = new Dictionary<FoodSource, List<LogEntry>>();
        this.enclosedAreaLogs = new Dictionary<EnclosedArea, List<LogEntry>>();
    }

    private void Start()
    {
        this.eventManager = EventManager.Instance;

        // Subscribe to the events
        this.eventManager.SubscribeToEvent(EventType.PopulationCountIncreased, () =>
        {
            this.handleLog(EventType.PopulationCountIncreased);
        });
        this.eventManager.SubscribeToEvent(EventType.PopulationCountDecreased, () =>
        {
            this.handleLog(EventType.PopulationCountDecreased);
        });
        this.eventManager.SubscribeToEvent(EventType.PopulationExtinct, () =>
        {
            this.handleLog(EventType.PopulationExtinct);
        });
        this.eventManager.SubscribeToEvent(EventType.NewPopulation, () =>
        {
            this.handleLog(EventType.NewPopulation);
        });
        this.eventManager.SubscribeToEvent(EventType.NewFoodSource, () =>
        {
            this.handleLog(EventType.NewFoodSource);
        });
        this.eventManager.SubscribeToEvent(EventType.NewEnclosedArea, () =>
        {
            this.handleLog(EventType.NewEnclosedArea);
        });
    }

    private void handleLog(EventType eventType)
    {
        if (eventType == EventType.PopulationCountIncreased)
        {
            this.logPopulationIncrease((Population)EventManager.Instance.LastEventInvoker);
        }
        else if (eventType == EventType.PopulationCountDecreased)
        {
            this.logPopulationDecrease((Population)EventManager.Instance.LastEventInvoker);
        }
        else if (eventType == EventType.PopulationExtinct)
        {
            this.logPopulationExtinct((Population)EventManager.Instance.LastEventInvoker);
        }
        else if (eventType == EventType.NewPopulation)
        {
            this.logNewCreation((Population)EventManager.Instance.LastEventInvoker);
        }
        else if (eventType == EventType.NewFoodSource)
        {
            this.logNewCreation((FoodSource)EventManager.Instance.LastEventInvoker);
        }
        else if (eventType == EventType.NewEnclosedArea)
        {
            this.logNewCreation((EnclosedArea)EventManager.Instance.LastEventInvoker);
        }
        else
        {
            Debug.Assert(true, $"LogSystem does not knows how to handle {eventType} yet");
        }
    }

    /// <summary>
    /// To handle toggling the window
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown("l"))
        {
            Debug.Log("open log");

            this.logWindow.SetActive(!this.isInLogSystem);
            this.isInLogSystem = !this.isInLogSystem;

            if (this.isInLogSystem)
            {
                this.displayWorldLog();
            }
        }
    }

    private void displayWorldLog()
    {
        string logText = "Log\n";

        if (this.worldLog.Count == 0)
        {
            this.logWindowText.text = "Log\n" + "None\n";
        }

        foreach(LogEntry logEntry in this.worldLog)
        {
            logText += $"{logEntry.GetDisplay()}\n";
        }

        this.logWindowText.text = logText;
    }

    private void logNewCreation(object creation)
    {
        if (creation.GetType() == typeof(Population))
        {
            Population population = (Population)creation;

            if (!this.populationLogs.ContainsKey(population))
            {
                this.populationLogs.Add(population, new List<LogEntry>());
            }

            LogEntry newLog = new LogEntry(Time.time.ToString(), $"New {population.species.SpeciesName} created");

            this.populationLogs[population].Add(newLog);
            this.worldLog.Add(newLog);
        }
        else if (creation.GetType() == typeof(FoodSource))
        {
            FoodSource foodSource = (FoodSource)creation;

            if (!this.foodSourceLogs.ContainsKey(foodSource))
            {
                this.foodSourceLogs.Add(foodSource, new List<LogEntry>());
            }

            LogEntry newLog = new LogEntry(Time.time.ToString(), $"New {foodSource.Species.SpeciesName} created");

            this.foodSourceLogs[foodSource].Add(newLog);
            this.worldLog.Add(newLog);
        }
        else if (creation.GetType() == typeof(EnclosedArea))
        {
            EnclosedArea enclosedArea = (EnclosedArea)creation;

            if (!this.enclosedAreaLogs.ContainsKey(enclosedArea))
            {
                this.enclosedAreaLogs.Add(enclosedArea, new List<LogEntry>());
            }

            LogEntry newLog = new LogEntry(Time.time.ToString(), $"Enclosed area {enclosedArea.id} created");

            this.enclosedAreaLogs[enclosedArea].Add(newLog);
            this.worldLog.Add(newLog);
        }
    }

    private void logPopulationIncrease(Population population)
    {
        if (!this.populationLogs.ContainsKey(population))
        {
            this.populationLogs.Add(population, new List<LogEntry>());
        }

        LogEntry newLog = new LogEntry(Time.time.ToString(), $"{population.species.SpeciesName} population size increased!");

        // Store to population log
        this.populationLogs[population].Add(newLog);
        // Store to world log
        this.worldLog.Add(newLog);
    }

    private void logPopulationDecrease(Population population)
    {
        if (!this.populationLogs.ContainsKey(population))
        {
            this.populationLogs.Add(population, new List<LogEntry>());
        }

        LogEntry newLog = new LogEntry(Time.time.ToString(), $"{population.species.SpeciesName} population size decreased!");

        // Store to population log
        this.populationLogs[population].Add(newLog);
        // Store to world log
        this.worldLog.Add(newLog);
    }

    private void logPopulationExtinct(Population population)
    {
        if (!this.populationLogs.ContainsKey(population))
        {
            this.populationLogs.Add(population, new List<LogEntry>());
        }

        LogEntry newLog = new LogEntry(Time.time.ToString(), $"{population.species.SpeciesName} has gone extinct!");

        this.populationLogs[population].Add(newLog);
        this.worldLog.Add(newLog);
    }
}
