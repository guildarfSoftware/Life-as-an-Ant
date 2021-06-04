
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Pheromones
{
    public enum PheromoneType
    {
        None,
        Harvest,
        Combat,
    }
    //@TODO Change first node to reference the source, leads somewhere will be false id the reference disapears
    public class PheromoneWaypoint : MonoBehaviour
    {
        int routeID;
        public float killTime { get; private set; }
        float elapsedTime;
        private const float timeToDestroy = 240;
        public int distanceFromSource = 0;
        bool leadsSomewhere = true; //indicates if the route ends somewhere with food or enemies true by default
        PheromoneWaypoint sourceWaypoint;
        public PheromoneWaypoint previousWaypoint;
        public PheromoneWaypoint nextWaypoint;      //points toward source: food or enemy
        public PheromoneType pheromoneType;

        private void Start()
        {
            killTime = timeToDestroy;
        }
        private void Update()
        {
            killTime -= Time.deltaTime;
            elapsedTime += Time.deltaTime;
            if (killTime <= 0) Destroy(gameObject);
        }

        public void SetPheromoneType(PheromoneType type)
        {
            pheromoneType = type;

            int pheromoneLayer;

            if (type == PheromoneType.Combat)
            {
                pheromoneLayer = LayerManager.pheromoneCombatLayer;
            }
            else
            {
                pheromoneLayer = LayerManager.pheromoneHarvestLayer;
            }

            gameObject.layer = pheromoneLayer;
        }

        public void UpdateKillTime()
        {
            if (leadsSomewhere) killTime = timeToDestroy;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            if (nextWaypoint != null)
                Gizmos.DrawLine(transform.position, nextWaypoint.transform.position);

            Gizmos.color = Color.black;
            if (previousWaypoint != null)
            {
                Gizmos.DrawLine(transform.position, previousWaypoint.transform.position);
            }
        }

        public void DisableRoute()
        {
            if (sourceWaypoint == null) return;
            sourceWaypoint.MarkAsInvalid();    // search the source node and marks it and all the following invalid
        }

        /*
        makes false leadSomewhere this node and all the following
        */
        public void MarkAsInvalid()
        {
            leadsSomewhere = false;
            killTime = distanceFromSource;

            TrailRenderer trailR = GetComponentInChildren<TrailRenderer>();
            if (trailR != null) trailR.time = elapsedTime + killTime;

            if (nextWaypoint != null) nextWaypoint.MarkAsInvalid();
        }

        public bool LeadsSomewhere()
        {
            if (sourceWaypoint == null) return false;
            return sourceWaypoint.leadsSomewhere;
        }

        public static PheromoneWaypoint CreatePheromoneWaypoint(PheromoneType type, PheromoneWaypoint previous, Vector3 position)
        {
            GameObject waypointObject = new GameObject();
            waypointObject.transform.position = position;

            waypointObject.AddComponent<SphereCollider>().isTrigger = true;

            PheromoneWaypoint waypointScript = waypointObject.AddComponent<PheromoneWaypoint>();
            waypointScript.SetPheromoneType(type);

            if (previous != null)
            {
                previous.nextWaypoint = waypointScript;
                waypointScript.previousWaypoint = previous;

                waypointScript.distanceFromSource = previous.distanceFromSource + 1;
                waypointScript.sourceWaypoint = previous.sourceWaypoint;

                waypointScript.killTime = previous.killTime + 1;
                waypointScript.leadsSomewhere = previous.leadsSomewhere;
            }
            else
            {
                waypointScript.sourceWaypoint = waypointScript;
            }

            string name = (type == PheromoneType.Combat ? "Combat" : "Harvest") + " Waypoint " + waypointScript.distanceFromSource;
            waypointObject.name = name;
            return waypointScript;
        }

        public static PheromoneWaypoint CreateSourceWaypoint(PheromoneType type, Vector3 position)
        {
            return CreatePheromoneWaypoint(type, null, position);
        }


    }
}