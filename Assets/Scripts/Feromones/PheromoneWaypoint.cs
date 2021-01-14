
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

        float strenght;
        public PheromoneWaypoint previousWaypoint;
        public PheromoneWaypoint nextWaypoint;      //points toward source: food or enemy
        public PheromoneType pheromoneType;

        private void Start()
        {
            strenght = 1;
        }

        private void Update()
        {
            if (pheromoneType == PheromoneType.Harvest)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            }
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