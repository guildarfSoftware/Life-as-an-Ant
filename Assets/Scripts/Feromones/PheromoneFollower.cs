using UnityEngine;
using RPG.Movement;
using System;
using RPG.Core;

namespace RPG.Pheromones
{
    public class PheromoneFollower : MonoBehaviour, IAction
    {
        public bool routeEnded;
        public bool AtSource
        {
            get
            {
                if (targetWaypoint != null) return targetWaypoint.distanceFromSource == 0;
                else return false;
            }
        }

        PheromoneWaypoint targetWaypoint;
        public PheromoneWaypoint lastWaypoint;
        float precision = 0.5f;
        private void Update()
        {
            if (targetWaypoint != null)
            {
                float distance = Vector3.Distance(transform.position, targetWaypoint.transform.position);
                if (distance <= precision)
                {
                    lastWaypoint = targetWaypoint;
                    targetWaypoint = lastWaypoint.previousWaypoint;
                    
                    lastWaypoint.UpdateKillTime();
                    targetWaypoint.UpdateKillTime();
                }
                else
                {
                    GetComponent<Mover>().MoveTo(targetWaypoint.transform.position);
                }
            }
            else
            {
                routeEnded = true;
            }
        }

        public void StartRoute(PheromoneWaypoint start)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            routeEnded = false;
            targetWaypoint = start;
        }

        public void StopRoute()
        {
            routeEnded = true;
            targetWaypoint = null;
        }

        internal bool CanFollow(GameObject target)
        {
            if (target == null) return false;

            PheromoneWaypoint waypoint = target.GetComponent<PheromoneWaypoint>();
            if (waypoint == null) return false;
            return waypoint.LeadsSomewhere();
        }

        public void Cancel()
        {
            routeEnded = true;
            lastWaypoint = null;
            targetWaypoint = null;
        }

        public void StartAction()
        {
            throw new NotImplementedException();
        }
    }
}