using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using System;
using RPG.Core;
using RPG.Harvest;
using RPG.Pheromones;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace RPG.Control
{
    [RequireComponent(typeof(Mover))]
    [RequireComponent(typeof(Fighter))]
    public class PlayerController : MonoBehaviour
    {
        private const int pheromoneTrailDuration = 90;
        Mover mover;
        Fighter fighter;
        Harvester harvester;
        Health health;
        EntityDetector detector;

        bool generatingPheromones;

        Leader leader;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            harvester = GetComponent<Harvester>();
            mover = GetComponent<Mover>();
            health = GetComponent<Health>();
            leader = GetComponent<Leader>();
            detector = GetComponentInChildren<EntityDetector>();

            harvester.fooodGrabbed += StartFoodPheromones;
            harvester.foodDeposit += StopPheromones;
            health.OnDeath += OnPlayerDeath;
            fighter.EnterCombat += harvester.DropFood;
        }
        void Update()
        {
            if (health.IsDead) return;
            if (EvaluateCombat()) return;
            if (EvaluateStorage()) return;
            if (EvaluateHarvest()) return;
            if (EvaluateMovement()) return;
        }


        private bool EvaluateCombat()
        {
            if (OverUIElement()) return false;

            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();  // todo refactored this due to changes in figther need to use combat target again 
                if (target == null || !fighter.CanAttack(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    fighter.Attack(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        private bool EvaluateStorage()
        {
            if (OverUIElement()) return false;

            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                Storage target = hit.transform.GetComponent<Storage>();


                if (target == null || !harvester.CanStore(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    harvester.Store(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        private bool EvaluateHarvest()
        {
            if (OverUIElement()) return false;

            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                HarvestTarget target = hit.transform.GetComponent<HarvestTarget>();
                if (target == null || !harvester.CanHarvest(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    harvester.Harvest(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        internal void Respawn(Vector3 position)
        {   
            mover.Warp(position);

            health.Revive();
        }

        private bool EvaluateMovement()
        {
            if (OverUIElement()) return false;

            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                GameObject target = hit.transform.gameObject;
                if (target == null || target.name != "Terrain") continue;

                if (Input.GetMouseButton(0))
                {
                    mover.StartMovement(hit.point);
                    //leader.FollowMeCommand();
                }
                return true;
            }
            return false;
        }
        void StartFoodPheromones()
        {
            if (PheromonesInRange(PheromoneType.Harvest)) return;

            GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Harvest);
            generatingPheromones = true;
        }

        void StartCombatPheromones()
        {
            if (PheromonesInRange(PheromoneType.Combat)) return;

            GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Combat);
            generatingPheromones = true;
        }


        public bool PheromonesInRange(PheromoneType type)
        {
            int pheromoneLayer;

            if(type==PheromoneType.Combat)
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
                if (waypoint.distanceFromSource == 0)
                {
                    return true;
                }

            }
            return false;
        }

        void StopPheromones()
        {
            GetComponent<PheromoneGenerator>().StopGeneration();
            generatingPheromones = false;
        }

        void OnPlayerDeath(GameObject gObject)
        {
            StopPheromones();
            transform.GetChild(0).localRotation = Quaternion.Euler(0, -90, 0);
        }


        private void OnDisable()
        {
            harvester.fooodGrabbed -= StartFoodPheromones;
            harvester.foodDeposit -= StopPheromones;
            health.OnDeath -= OnPlayerDeath;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        private bool OverUIElement()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}