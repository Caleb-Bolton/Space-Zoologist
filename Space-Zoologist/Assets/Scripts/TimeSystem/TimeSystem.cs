using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO create QueueChangeClass which gets invoked by Initialize and nextDay to calculate any changes
public class TimeSystem : MonoBehaviour
{
    [SerializeField] ReserveDraft ReserveDraft = default;
    [SerializeField] NeedSystemManager NeedSystemManager = default;
    [SerializeField] PopulationManager PopulationManager = default;
    [SerializeField] Inspector Inspector = default;
    [SerializeField] Text CurrentDayText = default;
    [SerializeField] BuildBufferManager buildBufferManager = default;
    private int currentDay = 1;
    private int maxDay = 20; //TODO implement max day?

    private void Start()
    {
        UpdateDayText(currentDay);
    }

    public void nextDay()
    {
        this.ReserveDraft.loadDraft();
        // Recalculates need system values and should updates all populations needs
        foreach (Population population in this.PopulationManager.Populations)
        {
            population.HandleGrowth();
        }
        this.NeedSystemManager.UpdateAllSystems();
        this.PopulationManager.UpdateAccessibleLocations();
        foreach (Population population in this.PopulationManager.Populations)
        {
            population.UpdateGrowthConditions();
        }
        this.Inspector.UpdateCurrentDisplay();
        this.buildBufferManager.CountDown();
        AudioManager.instance.PlayOneShot(SFXType.FastForward);
        UpdateDayText(++currentDay);
    }

    private void UpdateDayText(int day)
    {
        CurrentDayText.text = "DAY " + day;
        if (maxDay > 0)
        {
            CurrentDayText.text += " / " + maxDay;
        }
    }
}
