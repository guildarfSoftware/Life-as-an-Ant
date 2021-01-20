using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using System;
using RPG.Core;
using RPG.Harvest;

namespace RPG.Control
{
    [RequireComponent(typeof(Mover))]
    [RequireComponent(typeof(Fighter))]
    public class PlayerController : MonoBehaviour
    {
        Mover mover;
        Fighter fighter;
        Harvester harvester;
        Health health;

        Leader leader;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            harvester = GetComponent<Harvester>();
            mover = GetComponent<Mover>();
            health = GetComponent<Health>();
            leader = GetComponent<Leader>();
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
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();  // todo refactored this due to changes in figther need to use combat target again 
                if (target == null || !fighter.CanAttack(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    fighter.Attack(target.gameObject);
                }
                else if( Input.GetMouseButtonDown(1))
                {
                    leader.CommandAttack(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        private bool EvaluateStorage()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                Storage target = hit.transform.GetComponent<Storage>();
                if (target == null || !harvester.CanStore(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    harvester.Store(target.gameObject);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    leader.CommandStore(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        private bool EvaluateHarvest()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                HarvestTarget target = hit.transform.GetComponent<HarvestTarget>();
                if (target == null || !harvester.CanHarvest(target.gameObject)) continue;

                if (Input.GetMouseButtonDown(0))
                {
                    harvester.Harvest(target.gameObject);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    leader.CommandHarvest(target.gameObject);
                }
                return true;    //outside the click check to allow hover detection;
            }
            return false;
        }

        private bool EvaluateMovement()
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);

            if (hasHit)
            {
                if (Input.GetMouseButton(0))
                {
                    mover.StartMovement(hit.point);
                }
                return true;
            }
            return false;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}