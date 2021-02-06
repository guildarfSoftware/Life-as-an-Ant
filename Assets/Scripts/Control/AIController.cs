using System;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 6f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 0.5f;
        GameObject target;
        Fighter fighter;
        Health health;
        Mover mover;
        EntityDetector detector;
        Vector3 guardPosition;
        Vector3 lastKnownPosition;
        float timeSinceLastSawTarget = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Start()
        {
            detector = GetComponentInChildren<EntityDetector>();
            fighter = GetComponent<Fighter>();
            target = GetValidTarget();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
        }
        private void Update()
        {

            if (health.IsDead) return;

            target = GetValidTarget();
            
            if (target == null || !fighter.CanAttack(target))
            {
                EvaluatePatrol();

                return;
            }

            timeSinceLastSawTarget += Time.deltaTime;

            if (InChaseDistance() && fighter.CanAttack(target))
            {
                EvaluateAttack();
                timeSinceLastSawTarget = 0;
                lastKnownPosition = target.transform.position;
            }
            else if (timeSinceLastSawTarget < suspicionTime)
            {
                EvaluateSuspicion();
            }
            else
            {
                EvaluatePatrol();
            }
        }

        public void SetPatrolPath(PatrolPath patrolPath)
        {
            this.patrolPath = patrolPath;
        }

        public void SetGuardPosition(Vector3 position)
        {
            guardPosition = position;
        }
        private GameObject GetValidTarget()
        {
            if (detector == null) return null;

            List<GameObject> workerAnts = detector.GetEntitiesWithTag("Worker");
            foreach (GameObject target in workerAnts)
            {
                if (target != null && fighter.CanAttack(target)) return target;
            }

            target = detector.GetEntityWithTag("Player");

            if (target != null && fighter.CanAttack(target)) return target;

            return null;
        }

        private void EvaluateAttack()
        {
            fighter.Attack(target);
        }

        private void EvaluateSuspicion()
        {
            target = null;
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void EvaluatePatrol()
        {
            Vector3 nextPosition = guardPosition;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();

            }
            mover.StartMovement(nextPosition);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypointPosition(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextWaypointIndex(currentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());

            return distanceToWaypoint < waypointTolerance;

        }

        float GetDistanceToPlayer()
        {
            return Vector3.Distance(transform.position, target.transform.position);
        }

        bool InChaseDistance()
        {
            return GetDistanceToPlayer() < chaseDistance;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}