using UnityEngine;
using RPG.Core;
using RPG.Pheromones;
using RPG.Harvest;
using RPG.Combat;
using System;
using System.Collections.Generic;
using RPG.Movement;

namespace RPG.Control
{
    public class WorkerController : MonoBehaviour
    {
        EntityDetector detector;
        PheromoneFollower pheromoneFollower;
        Harvester harvester;
        Fighter fighter;
        Explorer explorer;
        GameObject nest;

        private void Start()
        {
            detector = GetComponent<EntityDetector>();
            pheromoneFollower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            nest = GameObject.FindGameObjectWithTag("Nest");
        }
        private void Update()
        {
            if (EvaluateStore()) return;
            if (EvaluateAttack()) return;
            if (EvaluateHarvest()) return;
            if (!pheromoneFollower.routeEnded) return; //following a pheromone trail

            DisableCurrentRoute(); //if got here means that trail leads to somewhere without a valid target. Disable it.

            if (EvaluateFindRoute()) return;

            if (EvaluateExplore()) return;

            GetComponent<Mover>().MoveTo(nest.transform.position);  // if nothing else can be done wait in the nest
        }


        private void DisableCurrentRoute()
        {
            if (pheromoneFollower.lastWaypoint != null)
            {
                pheromoneFollower.lastWaypoint.DisableRoute();  //route has ended and cannot find food or enemy
                pheromoneFollower.Cancel();
            }
        }

        bool EvaluateStore()
        {
            if (!harvester.IsEmpty && harvester.CanStore(nest))
            {
                harvester.Store(nest);
                return true;
            }
            return false;
        }

        bool EvaluateAttack()
        {
            GameObject target = GetClosestEntityWithTag("Enemy");
            if (target != null && fighter.CanAttack(target))
            {
                GetComponent<Fighter>().Attack(target);
                pheromoneFollower.Cancel();   // to avoid mark as invalid last waypoint
                return true;
            }
            return false;
        }
        bool EvaluateHarvest()
        {
            GameObject target = GetClosestEntityWithTag("Food");
            if (target != null && harvester.CanHarvest(target))
            {
                GetComponent<Harvester>().Harvest(target);
                pheromoneFollower.Cancel();   // to avoid mark as invalid last waypoint
                return true;
            }
            return false;

        }

        bool EvaluateFindRoute()
        {
            List<GameObject> waypoints = detector.GetEntitiesWithTag("PheromoneCombat");

            PheromoneWaypoint target = GetWaypointClosestToSource(waypoints);

            if (target != null && target.LeadsSomewhere())
            {
                pheromoneFollower.StartRoute(target.GetComponent<PheromoneWaypoint>());
                return true;
            }

            waypoints = detector.GetEntitiesWithTag("PheromoneHarvest");

            target = GetWaypointClosestToSource(waypoints);

            if (target != null && target.LeadsSomewhere())
            {
                pheromoneFollower.StartRoute(target.GetComponent<PheromoneWaypoint>());
                return true;
            }

            return false;
        }

        bool EvaluateExplore()
        {
            if (!explorer.onCooldown) explorer.Wander(); //explore for a while around position if not on cooldown

            if (!explorer.TimeOut)  //is exploring
            {
                return true;
            }

            return false;
        }

        PheromoneWaypoint GetWaypointClosestToSource(List<GameObject> waypoints)
        {
            PheromoneWaypoint returnedWaypoint = null;
            if (waypoints != null && waypoints.Count != 0)
            {
                returnedWaypoint = waypoints[0].GetComponent<PheromoneWaypoint>();

                foreach (GameObject waypointObject in waypoints)
                {
                    PheromoneWaypoint waypoint = waypointObject.GetComponent<PheromoneWaypoint>();
                    if (waypoint != null && waypoint.distanceFromSource < returnedWaypoint.distanceFromSource)
                    {
                        returnedWaypoint = waypoint;
                    }
                }
            }
            return returnedWaypoint;
        }

        private GameObject GetClosestEntityWithTag(string tag)
        {
            List<GameObject> list = detector.GetEntitiesWithTag(tag);

            list.Sort((a, b) => GetDistance(a).CompareTo(GetDistance(b)));  //sort by distance

            if (list == null || list.Count == 0) return null;

            return list[0];
        }

        private float GetDistance(GameObject gObject)
        {
            if (gObject == null) return -1;
            return Vector3.Distance(transform.position, gObject.transform.position);
        }
    }
}