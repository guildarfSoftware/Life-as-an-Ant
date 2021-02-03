using System;
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
        [SerializeField] EntityDetector detector;
        Vector3 guardPosition;
        Vector3 lastKnownPosition;
        float timeSinceLastSawTarget = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            target = GetValidTarget();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            guardPosition = transform.position;
        }
        private void Update()
        {

            if (health.IsDead) return;

            if (target == null || !fighter.CanAttack(target))
            {
                target = GetValidTarget();
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

        private GameObject GetValidTarget()
        {
            if (detector == null) return null;

            GameObject target = detector.GetEntityWithTag("Player");

            if (target != null && fighter.CanAttack(target)) return target;

            target = detector.GetEntityWithTag("Worker");

            if (target != null && fighter.CanAttack(target)) return target;

            return null;
        }

        private void EvaluateAttack()
        {
            fighter.Attack(target);
        }

        private void EvaluateSuspicion()
        {
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