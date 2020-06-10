﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AtmoshpereNeedSystem : NeedSystem
{
    //private readonly ReservePartitionManager rpm = null;
    private readonly EnclosureSystem enclosureSystem = null;
    public AtmoshpereNeedSystem(EnclosureSystem enclosureSystem, string needName = "Atmoshpere") : base(needName)
    {
        this.enclosureSystem = enclosureSystem;
    }

    /// <summary>
    /// Updates the density score of all the registered population and updates the associated need in the Population's needs
    /// </summary>
    public override void UpdateSystem()
    {
        foreach (Population population in populations)
        {
            // Get the atmospheric composition of a population 
            var atmosphericComposition = enclosureSystem.GetAtmosphericComposition(Vector3Int.FloorToInt(population.transform.position));

            Debug.Log(atmosphericComposition);
        }
    }
}
