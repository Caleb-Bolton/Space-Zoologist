﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedSystemsTester : MonoBehaviour
{
    [SerializeField] private PopulationManager populationManager = default;
    [SerializeField] private NeedSystemManager needSystemManager = default;
    [SerializeField] private List<GameObject> testSystemGameObjects = default;

    [SerializeField] private List<Species> species = default;

    // Start is called before the first frame update
    void Start()
    {
        if (!populationManager) populationManager = FindObjectOfType<PopulationManager>();
        foreach(GameObject testSystem in testSystemGameObjects)
        {
            needSystemManager.AddSystem(testSystem.GetComponent<TestSystem>());
        }
        foreach(Species s in species)
        {
            populationManager.CreatePopulation(s, Vector2Int.zero);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}