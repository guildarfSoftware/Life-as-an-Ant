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
        Health health;
        GameObject nest;
        float stopPheromoneDistance = 3f;
        bool generatingPheromones;

        private void Start()
        {
            detector = GetComponent<EntityDetector>();
            pheromoneFollower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            health = GetComponent<Health>();

            harvester.fooodGrabbed += StartFoodPheromones;
            fighter.EnterCombat += StartCombatPheromones;
            GetComponent<Health>().OnDamaged += StartCombatPheromones;

            nest = GameObject.FindGameObjectWithTag("Nest");
        }
        private void Update()
        {
            if (health.IsDead) return;

            if (EvaluateNotifyNest()) return;   //if generating pheromones return to nest to notify

            if (EvaluateStore()) return;

            if (EvaluateCombat()) return;
            if (EvaluateHarvest()) return;

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

        bool EvaluateNotifyNest()
        {
            if (!generatingPheromones) return false;
            GetComponent<Mover>().StartMovement(nest.transform.position);
            if (GetDistance(nest) < stopPheromoneDistance)
            {
                StopPheromones();
                return false;
            }
            return true;
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

        bool EvaluateCombat()
        {
            GameObject target = GetClosestEntityWithTag("Enemy");
            if (target != null && fighter.CanAttack(target))    //an enemy is close enought to attack
            {
                GetComponent<Fighter>().Attack(target);
                return true;
            }

            if (pheromoneFollower.lastWaypoint != null &&
                pheromoneFollower.lastWaypoint.pheromoneType == PheromoneType.Combat)
            {
                if (!pheromoneFollower.routeEnded)   //following a combat pheromone trail
                {
                    return true;
                }
                else    // combat trail ended without valid target
                {
                    DisableCurrentRoute();
                }
            }

            return FollowCloseTrail(PheromoneType.Combat);
        }


        bool EvaluateHarvest()
        {
            GameObject target = GetClosestEntityWithTag("Food");
            if (target != null && harvester.CanHarvest(target))
            {
                harvester.Harvest(target);
                return true;
            }

            if (pheromoneFollower.lastWaypoint != null &&
                pheromoneFollower.lastWaypoint.pheromoneType == PheromoneType.Harvest)
            {
                if (!pheromoneFollower.routeEnded)   //following a harvest pheromone trail
                {
                    return true;
                }
                else    // harvest trail ended without valid target
                {
                    DisableCurrentRoute();
                }
            }

            return FollowCloseTrail(PheromoneType.Harvest);

        }

        bool FollowCloseTrail(PheromoneType type)   // follows a trail of the type if exist return true
        {
            List<GameObject> waypoints;
            if (type == PheromoneType.Combat)
            {
                waypoints = detector.GetEntitiesWithTag("PheromoneCombat");
            }
            else
            {
                waypoints = detector.GetEntitiesWithTag("PheromoneHarvest");
            }

            PheromoneWaypoint target = GetWaypointClosestToSource(waypoints);

            if (target != null && target.LeadsSomewhere())
            {
                pheromoneFollower.StartRoute(target);
                return true;
            }

            return false;
        }

        bool EvaluateExplore()
        {
            if (!explorer.onCooldown && !explorer.wandering) explorer.Wander(); //start exploration behaviour

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


        void StartFoodPheromones()
        {
            if (detector.GetEntityWithTag("PheromoneHarvest") == null) //check to avoid multiple trails
            {
                GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Harvest);
                generatingPheromones = true;
            }
        }

        void StartCombatPheromones()
        {
            if (detector.GetEntityWithTag("PheromoneCombat") == null) //check to avoid multiple trails
            {
                GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Combat);
                generatingPheromones = true;
            }
        }


        void StopPheromones()
        {
            GetComponent<PheromoneGenerator>().StopGeneration();
            generatingPheromones = false;
        }


        private void OnDisable()
        {
            harvester.fooodGrabbed -= StartFoodPheromones;
            fighter.EnterCombat -= StartCombatPheromones;
            GetComponent<Health>().OnDamaged -= StartCombatPheromones;
        }
    }
}