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
        enclosureSystem.FindEnclosedAreas();

        foreach (Life life in lives)
        {
            // Get the atmospheric composition of a population 
            AtmosphericComposition atmosphericComposition = enclosureSystem.GetAtmosphericComposition(Vector3Int.FloorToInt(life.transform.position));

            // THe composition is a list of float value in the order of the AtmoshpereComponent Enum
            float[] composition = atmosphericComposition.GeComposition();
            
            foreach (var (value, index) in composition.WithIndex())
            {
                string needName = ((AtmoshpereComponent)index).ToString();

                if (life.NeedsValues.ContainsKey(needName))
                {
                    life.UpdateNeed(needName, value);
                }
            }
        }
    }
}