using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerManager
{

    static string playerLayerName = "Player";
    static string workerLayerName = "Worker";
    static string enemyLayerName = "Enemy";
    static string foodLayerName = "Food";
    static string detectorLayerName = "Detector";
    static string anthillLayerName = "Nest";
    static string pheromoneCombatLayerName = "PheromonesCombat";
    static string pheromoneHarvestLayerName = "PheromonesHarvest";


    public static int playerLayer = 8, workerLayer = 9, enemyLayer = 10, foodLayer = 11, detectorLayer = 12, anthillLayer = 13, pheromoneCombatLayer = 14, pheromoneHarvestLayer = 15;

    public static bool CheckLayerNamesConsistency()
    {
        if (LayerMask.NameToLayer(playerLayerName) != playerLayer) return false;
        if (LayerMask.NameToLayer(workerLayerName) != workerLayer) return false;
        if (LayerMask.NameToLayer(enemyLayerName) != enemyLayer) return false;
        if (LayerMask.NameToLayer(foodLayerName) != foodLayer) return false;
        if (LayerMask.NameToLayer(detectorLayerName) != detectorLayer) return false;
        if (LayerMask.NameToLayer(anthillLayerName) != anthillLayer) return false;
        if (LayerMask.NameToLayer(pheromoneCombatLayerName) != pheromoneCombatLayer) return false;
        if (LayerMask.NameToLayer(pheromoneHarvestLayerName) != pheromoneHarvestLayer) return false;
        return true;
    }

}
