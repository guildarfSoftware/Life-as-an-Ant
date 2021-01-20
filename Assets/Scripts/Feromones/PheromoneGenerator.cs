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
        bool safeExit;
        float timeBetweenWaypoints = 0.5f;
        [SerializeField] bool generating;
        PheromoneType generatingType;

        Harvester harvester;
        Fighter fighter;
        PheromoneWaypoint lastGenerated = null;
        float pheromoneDuration;
        const float combatCooldown = 15f;

        private void Start()
        {
            StartCoroutine(GenerationProcess());

            harvester = GetComponent<Harvester>();
            harvester.fooodGrabbed += () => { StartGeneration(PheromoneType.Harvest); };
            harvester.foodDeposit += StopGeneration;

            fighter = GetComponent<Fighter>();
            fighter.EnterCombat += () => { StartGeneration(PheromoneType.Combat, combatCooldown); };

            Health health = GetComponent<Health>();
            health.OnDamaged += () => { StartGeneration(PheromoneType.Combat, combatCooldown); };
        }

        private void Update()
        {
            pheromoneDuration -= Time.deltaTime;
            if ( pheromoneDuration < 0)
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

        public void StartGeneration(PheromoneType type, float duration = -1)
        {

            if (PheromoneSourceInRange(type)) return;

            generating = true;

            if (duration != -1)
            {
                pheromoneDuration = duration;
            }
            else
            {
                pheromoneDuration = float.MaxValue;
            }

            if (type == generatingType) return;
            lastGenerated = null;
            generatingType = type;

        }

        public void StopGeneration()
        {
            generating = false;
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