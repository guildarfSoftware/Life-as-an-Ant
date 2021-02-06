using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Harvest;
using RPG.Movement;
using RPG.Pheromones;
using UnityEngine;

namespace RPG.Control
{
    public class Follower : MonoBehaviour
    {
        Health health;
        EntityDetector detector;
        PheromoneFollower pheromoneFollower;
        Harvester harvester;
        Fighter fighter;
        Explorer explorer;
        PheromoneGenerator pheromoneGenerator;
        Mover mover;
        private PheromoneType pheromoneType;
        GameObject target;

        GameObject nest, leader;
        AntState currentState;
        float targetRange = 2f;
        float followRange = 5f;
        private float maxPlayerDistance = 6f;

        enum AntState   //ordere by priority
        {
            notifyingNest,
            moveToSource,
            attacking,
            storing,
            harvesting,
            followingLeader,
            iddle,
        }

        private void Start()
        {
            health = GetComponent<Health>(); 
            pheromoneGenerator = GetComponent<PheromoneGenerator>();
            detector = GetComponentInChildren<EntityDetector>();
            pheromoneFollower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            mover = GetComponent<Mover>();

            nest = GameObject.FindGameObjectWithTag("Nest");
            leader = GameObject.FindGameObjectWithTag("Player");

            FollowTheLeader();
        }
        private void Update()
        {
            if(health.IsDead) 
            {
                Destroy(gameObject, 30);
                return;
            }
            if (target == null) currentState = AntState.iddle;

            if (!isNotifying() && LeaderIsTooFar()) FollowTheLeader();

            switch (currentState)
            {
                case AntState.moveToSource:
                    {
                        mover.MoveTo(target.transform.position);
                        if (GetDistance(target) < targetRange)
                        {
                            if (StartPheromones()) MoveToNest();
                            else
                            {
                                currentState = AntState.iddle;
                            }

                        }
                        break;
                    }
                case AntState.notifyingNest:
                    {
                        mover.MoveTo(nest.transform.position);
                        if (GetDistance(nest) < targetRange)
                        {
                            pheromoneGenerator.StopGeneration();
                            pheromoneType = PheromoneType.None;
                            currentState = AntState.iddle;
                        }
                        break;
                    }
                case AntState.attacking:
                    {
                        fighter.Attack(target);
                        if (target.transform.GetComponent<Health>().IsDead)
                        {
                            currentState = AntState.iddle;
                        }
                        break;
                    }
                case AntState.storing:
                    {
                        harvester.Store(target);
                        if (harvester.IsEmpty)
                        {
                            currentState = AntState.iddle;
                        }

                        break;
                    }
                case AntState.harvesting:
                    {
                        harvester.Harvest(target);
                        if (harvester.IsFull || target.GetComponent<HarvestTarget>().IsEmpty)
                        {
                            currentState = AntState.iddle;
                        }
                        break;
                    }
                case AntState.followingLeader:
                    {
                        target = leader;
                        if (target == null) currentState = AntState.iddle;

                        Vector3 followPosition = GetFollowPosition(target);
                        mover.MoveTo(followPosition);
                        break;
                    }
                case AntState.iddle:
                {
                    if(target== null) FollowTheLeader();
                    else if (harvester.CanHarvest(target)) ChangeState(AntState.harvesting);
                    else if (fighter.CanAttack(target)) ChangeState(AntState.attacking);
                    else FollowTheLeader();
                    break;
                }
                default:
                    {
                        FollowTheLeader();
                        break;
                    }
            }
        }

        private bool StartPheromones()
        {
            if (detector != null)
            {
                string tag = pheromoneType == PheromoneType.Harvest ? "PheromoneHarvest" : "PheromoneCombat";
                List<GameObject> waypoints = detector.GetEntitiesWithTag(tag);
                if (waypoints.Count != 0)
                {
                    return false;
                }
            }
            pheromoneGenerator.StartGeneration(pheromoneType);
            return true;
        }

        private bool LeaderIsTooFar()
        {
            float distanceToPlayer = GetDistance(leader);

            return distanceToPlayer > maxPlayerDistance;

        }

        private Vector3 GetFollowPosition(GameObject target)    // follows at exactly follow rango to avoid followers making barrier
        {
            Vector3 followDirection = target.transform.position - transform.position;
            followDirection.Normalize();
            return target.transform.position - followDirection * followRange;
        }

        private float GetDistance(GameObject target)
        {
            return Vector3.Distance(target.transform.position, transform.position);
        }

        public bool Harvest(GameObject target)
        {
            if (ChangeState(AntState.harvesting))
            {
                this.target = target;
                harvester.Harvest(target);
                return true;
            }
            return false;

        }
        public bool Attack(GameObject target)
        {
            if (ChangeState(AntState.moveToSource))
            {
                this.target = target;
                fighter.Attack(target);
                return true;
            }
            return false;
        }
        public bool Notify(PheromoneType pheromoneType, GameObject source)
        {
            if (ChangeState(AntState.moveToSource))
            {
                this.pheromoneType = pheromoneType;
                target = source;
                return true;
            }
            return false;
        }

        public bool MoveToNest()
        {
            if (ChangeState(AntState.notifyingNest))
            {
                target = nest;
                GetComponent<Mover>().MoveTo(target.transform.position);
                return true;
            }
            return false;
        }

        public bool Store(GameObject target)
        {
            if (ChangeState(AntState.storing))
            {
                this.target = target;
                harvester.Store(target);
                return true;
            }
            return false;
        }

        public bool FollowTheLeader()
        {
            if (ChangeState(AntState.followingLeader))
            {
                target = leader;
                return true;
            }
            return false;
        }

        private bool ChangeState(AntState newState)
        {
            if (newState < currentState)    // can only change to a more prioritary state
            {
                GetComponent<ActionScheduler>().CancelCurrentAction();
                currentState = newState;
                return true;
            }
            return false;
        }

        bool isNotifying()
        {
            return currentState == AntState.notifyingNest || currentState == AntState.moveToSource;
        }

    }

}