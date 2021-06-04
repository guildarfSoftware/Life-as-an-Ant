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

        public PheromoneWaypoint TargetWaypoint{get=> targetWaypoint;}
        public PheromoneWaypoint LastWaypoint { get => lastWaypoint; }
        PheromoneWaypoint targetWaypoint;
        PheromoneWaypoint lastWaypoint;
        float precision = 1.5f;
        public PheromoneType PheromoneType{get=>pheromoneType;}
        PheromoneType pheromoneType;

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
                    if (targetWaypoint != null) targetWaypoint.UpdateKillTime();
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
            if (!GetComponent<ActionScheduler>().StartAction(this)) return;
            routeEnded = false;
            lastWaypoint = start;
            targetWaypoint = start;
            pheromoneType = start.pheromoneType;
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
            pheromoneType = PheromoneType.None;
        }

        public bool isCancelable()
        {
            return true;
        }
    }
}