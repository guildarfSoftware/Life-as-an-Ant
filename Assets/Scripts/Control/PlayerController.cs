using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using System;
using RPG.Core;
using RPG.Harvest;
using RPG.Pheromones;
using UnityEngine.EventSystems;

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
            fighter.EnterCombat += StartCombatPheromones;
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
                    // }
                    // else if (Input.GetMouseButtonDown(1))
                    // {
                    leader.CommandNotify(PheromoneType.Combat, target.gameObject);
                    leader.CommandAttack(target.gameObject);
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
                    // }
                    // else if (Input.GetMouseButtonDown(1))
                    // {
                    leader.CommandStore(target.gameObject);
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
                    // }
                    // else if (Input.GetMouseButtonDown(1))
                    // {
                    leader.CommandNotify(PheromoneType.Harvest, target.gameObject);
                    leader.CommandHarvest(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
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
            if (detector.GetEntityWithTag("PheromoneHarvest") == null) //check to avoid multiple trails
            {
                GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Harvest, pheromoneTrailDuration);
                generatingPheromones = true;
            }
        }

        void StartCombatPheromones()
        {
            if (detector.GetEntityWithTag("PheromoneCombat") == null) //check to avoid multiple trails
            {
                GetComponent<PheromoneGenerator>().StartGeneration(PheromoneType.Combat, pheromoneTrailDuration);
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
            harvester.foodDeposit -= StopPheromones;
            fighter.EnterCombat -= StartCombatPheromones;
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