
using System;
using UnityEngine;

namespace RPG.Pheromones
{
    public enum PheromoneType
    {
        Harvest,
        Combat,
    }

    public class PheromoneWaypoint : MonoBehaviour
    {
        private float killTime;
        private const float timeToDestroy = 45;
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
            if (killTime <= 0) Destroy(gameObject);
        }

        public void SetPheromoneType(PheromoneType type)
        {
            pheromoneType = type;
            if (pheromoneType == PheromoneType.Harvest)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }

        public void UpdateKillTime()
        {
            killTime = timeToDestroy;
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

        internal void MarkAsInvalid()
        {
            leadsSomewhere=false;
            if(previousWaypoint!= null) previousWaypoint.MarkAsInvalid();
        }

        public bool LeadsSomewhere()
        {
            if(!leadsSomewhere) return false;   //node marked as invalid
            if(previousWaypoint != null) return previousWaypoint.LeadsSomewhere(); // check if node closer to source is valid 
            return true;
        }
    }
}