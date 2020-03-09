﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSystem : MonoBehaviour, INeedSystem
{

    private List<Population> Populations = new List<Population>();
    public readonly string NeedName = "space_maple";
    [SerializeField] private Text populationsText = default;

    string INeedSystem.NeedName => NeedName;

    public void RegisterPopulation(Population population)
    {
        Populations.Add(population);
    }

    public void UnregisterPopulation(Population population)
    {
        Populations.Remove(population);
    }

    public void UpdatePopulations()
    {
        foreach (Population population in Populations)
        {
            population.UpdateNeed(NeedName, CalculateNeedValue(population));
        }
    }

    private float CalculateNeedValue(Population population)
    {
        return Random.value;
    }

    private void Update()
    {
        string text = "";
        foreach (Population population in Populations)
        {
            text += population.SpeciesName;
        }
        populationsText.text = text;
    }
}
