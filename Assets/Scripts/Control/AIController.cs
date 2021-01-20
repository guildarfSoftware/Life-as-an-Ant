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
        GameObject player;
        Fighter fighter;
        Health health;
        Mover mover;

        Vector3 guardPosition;
        Vector3 lastKnownPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            player = GameObject.FindWithTag("Player");
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            guardPosition = transform.position;
        }
        private void Update()
        {
            if (health.IsDead) return;

            timeSinceLastSawPlayer += Time.deltaTime;

            if (InChaseDistance() && fighter.CanAttack(player))
            {
                EvaluateAttack();
                timeSinceLastSawPlayer = 0;
                lastKnownPosition = player.transform.position;
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                EvaluateSuspicion();
            }
            else
            {
                EvaluatePatrol();
            }
        }

        private void EvaluateAttack()
        {
            fighter.Attack(player);
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

        private void EvaluateCombat()
        {
        }

        float GetDistanceToPlayer()
        {
            return Vector3.Distance(transform.position, player.transform.position);
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