using UnityEngine;
using RPG.Harvest;
using RPG.Combat;
using System.Collections;
using RPG.Core;
using System;
using System.Collections.Generic;

namespace RPG.Pheromones
{
    public class PheromoneGenerator : MonoBehaviour
    {
        //[SerializeField] GameObject pheromonePrefab;
        bool safeExit;
        float timeBetweenWaypoints = 0.5f;
        [SerializeField] bool generating;
        PheromoneType generatingType;

        Harvester harvester;
        Fighter fighter;
        PheromoneWaypoint lastGenerated = null;
        float timeOutOfCOmbat;
        const float combatCooldown = 15;

        private void Start()
        {
            StartCoroutine(GenerationProcess());

            harvester = GetComponent<Harvester>();
            harvester.fooodGrabbed += () => { StartGeneration(PheromoneType.Harvest); };
            harvester.foodDeposit += () => { generating = false; };

            fighter = GetComponent<Fighter>();
            fighter.EnterCombat += () => { StartGeneration(PheromoneType.Combat); };
        }

        private void Update()
        {
            timeOutOfCOmbat += Time.deltaTime;
            if (generatingType == PheromoneType.Combat && timeOutOfCOmbat > combatCooldown)
            {
                generating = false;
            }
        }

        IEnumerator GenerationProcess()
        {
            while (!safeExit)
            {
                if (generating)
                {
                    PheromoneWaypoint newWaypoint = CreatePheromoneWaypoint();

                    if (lastGenerated != null)
                    {
                        newWaypoint.distanceFromSource = lastGenerated.distanceFromSource + 1;
                        lastGenerated.nextWaypoint = newWaypoint;
                    }
                    newWaypoint.previousWaypoint = lastGenerated;
                    lastGenerated = newWaypoint;

                    yield return new WaitForSeconds(timeBetweenWaypoints);
                }
                yield return null;
            }
        }

        private PheromoneWaypoint CreatePheromoneWaypoint()
        {
            GameObject waypointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypointObject.transform.position = transform.position;

            (waypointObject.AddComponent<SphereCollider>()).isTrigger = true;
            waypointObject.tag = generatingType == PheromoneType.Combat ? "PheromoneCombat" : "PheromoneHarvest";

            PheromoneWaypoint waypointScript = waypointObject.AddComponent<PheromoneWaypoint>();
            waypointScript.SetPheromoneType(generatingType);

            return waypointScript;
        }

        void StartGeneration(PheromoneType type)
        {

            if (PheromoneSourceInRange(type)) return;

            generating = true;

            if (type == PheromoneType.Combat)
            {
                timeOutOfCOmbat = 0;
            }

            if (type == generatingType) return;
            lastGenerated = null;
            generatingType = type;

        }

        bool PheromoneSourceInRange(PheromoneType type)
        {
            EntityDetector detector = GetComponent<EntityDetector>();
            if(detector ==null) return false;
            string pheromoneTag = ((type == PheromoneType.Combat) ? "PheromoneCombat" : "PheromoneHarvest");
            List<GameObject> waypoints = detector.GetEntitiesWithTag(pheromoneTag);

            if (waypoints == null || waypoints.Count == 0) return false;

            foreach (GameObject pheromoneObject in waypoints)
            {
                PheromoneWaypoint waypoint = pheromoneObject.transform.GetComponent<PheromoneWaypoint>();

                if (waypoint == null) continue;

                if (waypoint.distanceFromSource == 0 && waypoint.LeadsSomewhere()) return true;

            }

            return false;

        }

    }
}