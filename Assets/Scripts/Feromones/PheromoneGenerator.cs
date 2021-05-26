using UnityEngine;
using System.Collections;
using RPG.Core;
using System;
using System.Collections.Generic;

namespace RPG.Pheromones
{
    public class PheromoneGenerator : MonoBehaviour
    {

        static Dictionary<int, GameObject> routes = new Dictionary<int, GameObject>();
        static int routeCount;
        static GameObject routesContainer;
        GameObject currentRoute;
        GameObject CurrentRoute
        {
            get
            {
                if (currentRoute == null)
                {
                    currentRoute = NewRoute();
                }
                return currentRoute;
            }
        }
        bool safeExit;
        float timeBetweenWaypoints = 0.5f;
        [SerializeField] bool generating;
        PheromoneType generatingType;
        PheromoneWaypoint lastGenerated = null;
        float pheromoneDuration;
        const float combatCooldown = 15f;

        [SerializeField] Gradient gradientHarvest, gradientCombat;
        [SerializeField] Material trailMaterial;

        private void Start()
        {
            if(routesContainer == null) routesContainer = new GameObject("Routes");
            StartCoroutine(GenerationProcess());
        }

        private void Update()
        {
            pheromoneDuration -= Time.deltaTime;
            if (pheromoneDuration < 0)
            {
                generating = false;
            }
        }

        IEnumerator GenerationProcess()
        {
            while (!safeExit)
            {
                if(generating)
                {
                    AddPheromoneWaypoint(transform.position);
                   yield return new WaitForSeconds(timeBetweenWaypoints);
                }
                yield return null;
            }
        }

        public void StartGeneration(PheromoneType type, float duration = -1)
        {
            if (type == PheromoneType.None) return;
            if (duration != -1)
            {
                pheromoneDuration = duration;
            }
            else
            {
                pheromoneDuration = float.MaxValue;
            }

            if (type != generatingType)
            {
                StopGeneration(); //Resets trail
                generatingType = type;
            }

            currentRoute = NewRoute();
            generating = true;
        }

        public void StopGeneration(Transform endPosition = null)
        {
            if (!generating) return;
            if (endPosition != null)
            {
                 AddPheromoneWaypoint(endPosition.position);
            }

            StopTrailGenerator();
            
            currentRoute = null;
            generating = false;
            lastGenerated = null;
        }

        private void AddPheromoneWaypoint(Vector3 position)
        {
            PheromoneWaypoint newWaypoint;

            if (lastGenerated != null)
            {
                newWaypoint = PheromoneWaypoint.CreatePheromoneWaypoint(generatingType, lastGenerated, position);

            }
            else //first waypoint of trail
            {
                newWaypoint = PheromoneWaypoint.CreateSourceWaypoint(generatingType, position);
                StartTrailGenerator();
            }

            lastGenerated = newWaypoint;

            if (!newWaypoint.LeadsSomewhere())
            {
                StopGeneration();   //stop generating for a broken route
            }

            newWaypoint.transform.SetParent(CurrentRoute.transform);

        }

        private void StartTrailGenerator()
        {
            GameObject trailGenerator = new GameObject("Trail Generator");

            GameObject trailRoute = currentRoute;

            trailGenerator.AddComponent<OnDestroyListener>().AddListener(()=>{Destroy(trailRoute);});

            TrailRenderer trailRenderer = trailGenerator.AddComponent<TrailRenderer>();
            trailRenderer.time = float.MaxValue;
            trailRenderer.colorGradient = (generatingType == PheromoneType.Combat) ? gradientCombat : gradientHarvest;
            trailRenderer.material = trailMaterial;

            trailGenerator.transform.position = this.transform.position;
            trailGenerator.transform.parent = this.transform;
        }

        void StopTrailGenerator()//puts the trail follower in the last waypoint generator so they are destroyed together
        {
            TrailRenderer trailGenerator = GetComponentInChildren<TrailRenderer>();

            if (trailGenerator != null)
            {
                trailGenerator.emitting = false;
                if (lastGenerated != null)
                {
                    trailGenerator.transform.parent = lastGenerated.transform;
                }
                else
                {
                    trailGenerator.time = 5;
                    Destroy(trailGenerator.gameObject, 5);
                }
            }
        }

        static GameObject NewRoute()
        {
            routeCount++;
            GameObject newRoute = new GameObject("Route " + routeCount);
            newRoute.transform.SetParent(routesContainer.transform);
            routes.Add(routeCount, newRoute);
            return newRoute;
        }

    }
}