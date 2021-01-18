
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
        private const float timeToDestroy = 25;
        public int distanceFromSource=0;
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
            if(killTime <= 0) Destroy(gameObject);
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

    }
}