using UnityEngine;
using RPG.Core;
using RPG.Pheromones;
using RPG.Harvest;
using RPG.Combat;
using System;
using System.Collections.Generic;
using RPG.Movement;
using MyTools;

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
        Mover mover;
        GameObject nest;
        float exploreCounter;
        float exploreTime = 10;
        float nestEntranceRange = 1f;
        float stopPheromoneDistance = 3f;
        bool generatingPheromones;
        bool orderToReturn;
        public event Action<bool, GameObject> EnterAnthill;

        AnthillMission mission;
        bool needsRest;

        enum AnthillMission
        {
            None,
            Rest,
            Build,
        }

        private void Awake()
        {
            detector = GetComponentInChildren<EntityDetector>();
            pheromoneFollower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            nest = GameObject.FindGameObjectWithTag("Nest");
        }

        private void OnEnable()
        {
            health.OnDeath += CreateCorpse;
            harvester.fooodGrabbed += StartFoodPheromones;
            fighter.EnterCombat += CheckCombatStatus;
            fighter.EnterCombat += harvester.DropFood;

            orderToReturn = false;
            mission = AnthillMission.None;
            if (health.IsDead)
            {
                health.Revive();
            }
        }

        private void OnDisable()
        {
            generatingPheromones = false;
            health.OnDeath -= CreateCorpse;
            GetComponent<PheromoneGenerator>().StopGeneration();
            if (harvester != null) harvester.fooodGrabbed -= StartFoodPheromones;
            if (fighter != null)
            {
                fighter.EnterCombat -= CheckCombatStatus;
                fighter.EnterCombat -= harvester.DropFood;
            }
            detector.Reset();
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void Update()
        {
            exploreCounter -= Time.deltaTime;

            if (health.IsDead)
            {
                return;
            }

            if (EvaluateMakePheromonePath()) return;   //if generating pheromones return to nest to notify

            if (EvaluateStore()) return;

            if (mission == AnthillMission.Build && EvaluateReturnToNest()) return;

            if (EvaluateCombat()) return;
            if (EvaluateHarvest()) return;

            if (EvaluateReturnToNest()) return;

            if (EvaluateExplore()) return;
        }



        private bool EvaluateReturnToNest()
        {
            if (!orderToReturn || mission == AnthillMission.None) return false;

            mover.StartMovement(nest.transform.position);
            float distanceToNest = Vector3.Distance(nest.transform.position, transform.position);

            if (distanceToNest < nestEntranceRange)
            {
                needsRest = false;
                bool isBuilder = mission == AnthillMission.Build;
                orderToReturn = false;
                mission = AnthillMission.None;
                EnterAnthill?.Invoke(isBuilder, gameObject);
            }

            return true;
        }

        private void CreateCorpse(GameObject gObject)
        {
            Transform bodyTransform = transform.GetChild(0);
            GameObject corpse = Instantiate(bodyTransform.gameObject, bodyTransform.position, bodyTransform.rotation);
            GameObject.Destroy(corpse, 30);
            transform.GetChild(0).localRotation = Quaternion.Euler(0, -90, 0);
        }


        private void DisableCurrentRoute()
        {
            if (pheromoneFollower.LastWaypoint != null)
            {
                pheromoneFollower.LastWaypoint.DisableRoute();  //route has ended and cannot find food or enemy
                pheromoneFollower.Cancel();
            }
        }

        bool EvaluateMakePheromonePath()
        {
            if (!generatingPheromones) return false;
            GetComponent<Mover>().StartMovement(nest.transform.position);

            float distanceToNest = Vector3.Distance(nest.transform.position, transform.position);

            if (distanceToNest < stopPheromoneDistance)
            {
                GetComponent<PheromoneGenerator>().StopGeneration(nest.transform);
                generatingPheromones = false;
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
            GameObject target = GetClosestEnemy();
            if (target != null && fighter.CanAttack(target))    //an enemy is close enought to attack
            {
                fighter.Attack(target);
                return true;
            }

            if (pheromoneFollower.LastWaypoint != null &&
                pheromoneFollower.PheromoneType == PheromoneType.Combat)
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

        private GameObject GetClosestEnemy()
        {
            var enemies = detector.GetEntitiesInLayer(LayerManager.enemyLayer);

            GameObject closestEnemy = null;
            float closestDistance = 0;
            if (enemies != null && enemies.Count != 0)
            {
                foreach (GameObject enemy in enemies)
                {
                    if (enemy == null || !fighter.CanAttack(enemy)) continue;

                    float distance = Tools.GetSquareDistance(gameObject, enemy);
                    if (closestEnemy == null || distance < closestDistance)
                    {
                        closestEnemy = enemy;
                        closestDistance = distance;
                    }
                }
            }
            return closestEnemy;
        }

        void CheckCombatStatus()
        {
            if (health.currentHealth < 6)
            {
                StartCombatPheromones();
            }
        }

        public void ReturnToBuild()
        {
            orderToReturn = true;
            mission = AnthillMission.Build;
        }
        public void ReturnToRest()
        {
            orderToReturn = true;
            mission = AnthillMission.Rest;
        }

        bool EvaluateHarvest()
        {
            GameObject target = detector.GetClosestEntityInLayer(LayerManager.foodLayer);
            if (target != null && harvester.CanHarvest(target))
            {
                harvester.Harvest(target);
                return true;
            }

            if (pheromoneFollower.LastWaypoint != null &&
                pheromoneFollower.PheromoneType == PheromoneType.Harvest)
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
            int layer;
            if (type == PheromoneType.Combat)
            {
                layer = LayerManager.pheromoneCombatLayer;
            }
            else
            {
                layer = LayerManager.pheromoneHarvestLayer;
            }

            var waypoints = detector.GetEntitiesInLayer(layer);

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
            if (!explorer.wandering)
            {
                explorer.Wander(); //start exploration behaviour
                exploreCounter = exploreTime;
            }

            if (exploreCounter < 0)
            {
                explorer.Cancel();
                ReturnToRest();
                return false;
            }


            return true;
        }

        PheromoneWaypoint GetWaypointClosestToSource(ICollection<GameObject> waypoints)
        {
            PheromoneWaypoint returnedWaypoint = null;
            if (waypoints != null && waypoints.Count != 0)
            {
                foreach (GameObject waypointObject in waypoints)
                {
                    PheromoneWaypoint waypoint = waypointObject.GetComponent<PheromoneWaypoint>();
                    if (waypoint == null || !waypoint.LeadsSomewhere()) continue;

                    if (returnedWaypoint == null || waypoint.distanceFromSource < returnedWaypoint.distanceFromSource)
                    {
                        returnedWaypoint = waypoint;
                    }
                }
            }
            return returnedWaypoint;
        }

        void StartFoodPheromones()
        {
            if (generatingPheromones || PheromonesInRange(PheromoneType.Harvest)) return;

            GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Harvest);
            generatingPheromones = true;
        }

        void StartCombatPheromones()
        {
            if (generatingPheromones || PheromonesInRange(PheromoneType.Combat)) return;

            GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Combat);
            generatingPheromones = true;
        }


        public bool PheromonesInRange(PheromoneType type)
        {
            int pheromoneLayer;

            if (type == PheromoneType.Combat)
            {
                pheromoneLayer = LayerManager.pheromoneCombatLayer;
            }
            else
            {
                pheromoneLayer = LayerManager.pheromoneHarvestLayer;
            }

            var waypoints = detector.GetEntitiesInLayer(pheromoneLayer);
            foreach (GameObject waypointObject in waypoints)
            {
                PheromoneWaypoint waypoint = waypointObject.GetComponent<PheromoneWaypoint>();
                if (waypoint.pheromoneType == type && waypoint.distanceFromSource == 0)
                {
                    return true;
                }

            }
            return false;
        }
    }
}