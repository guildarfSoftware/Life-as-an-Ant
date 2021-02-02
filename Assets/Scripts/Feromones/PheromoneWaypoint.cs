
using System;
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
        public float killTime { get; private set;}
        float elapsedTime;
        private const float timeToDestroy = 240;
        public int distanceFromSource = 0;
        bool leadsSomewhere = true; //indicates if the route ends somewhere with food or enemies true by default
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
            // if (pheromoneType == PheromoneType.Harvest)
            // {
            //     gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            // }
            // else
            // {
            //     gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            // }
        }

        public void UpdateKillTime()
        {
            if(leadsSomewhere) killTime = timeToDestroy;
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
            GetSourceWaypoint().MarkAsInvalid();    // search the source node and marks it and all the following invalid
        }

        /*
        makes false leadSomewhere this node and all the following
        */
        private void MarkAsInvalid()
        {
            leadsSomewhere = false;
            killTime = distanceFromSource;

            TrailRenderer trailR = GetComponentInChildren<TrailRenderer>();
            if(trailR!=null) trailR.time = elapsedTime + killTime;

            if (nextWaypoint != null) nextWaypoint.MarkAsInvalid();
        }

        public PheromoneWaypoint GetSourceWaypoint()
        {
            if (previousWaypoint == null) return this;
            return previousWaypoint.GetSourceWaypoint();
        }

        public bool LeadsSomewhere()
        {
            if (!leadsSomewhere) return false;   //node marked as invalid
            if (previousWaypoint != null) return previousWaypoint.LeadsSomewhere(); // check if node closer to source is valid 
            return true;
        }
    }
}