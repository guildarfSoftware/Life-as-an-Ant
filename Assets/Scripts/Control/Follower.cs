﻿using System;
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
        EntityDetector detector;
        PheromoneFollower pheromoneFollower;
        Harvester harvester;
        Fighter fighter;
        Explorer explorer;
        PheromoneGenerator pheromoneGenerator;
        Mover mover;
        GameObject target;

        GameObject nest, player;
        AntState currentState;
        float targetRange = 2f;
        float followRange= 5f;
        enum AntState   //ordere by priority
        {
            notifying,
            attacking,
            storing,
            harvesting,
            following,
        }

        private void Start()
        {
            pheromoneGenerator = GetComponent<PheromoneGenerator>();
            detector = GetComponent<EntityDetector>();
            pheromoneFollower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            mover=  GetComponent<Mover>();

            nest = GameObject.FindGameObjectWithTag("Nest");
            player = GameObject.FindGameObjectWithTag("Player");

            FollowTheLeader();
        }
        private void Update()
        {
            if(target == null) FollowTheLeader();
            switch (currentState)
            {
                case AntState.notifying:
                    {
                        mover.MoveTo(target.transform.position);
                        if(GetDistance(target) < targetRange)
                        {
                            pheromoneGenerator.StopGeneration();
                            FollowTheLeader();
                        }
                        break;
                    }
                case AntState.attacking:
                    {
                        fighter.Attack(target);
                        if(target.transform.GetComponent<Health>().IsDead)
                        {
                            FollowTheLeader();
                        }
                        break;
                    }
                case AntState.storing:
                    {
                        harvester.Store(target);

                        if(harvester.IsEmpty)
                        {
                            FollowTheLeader();
                        }

                        break;
                    }
                case AntState.harvesting:
                    {
                        harvester.Harvest(target);
                        if(harvester.IsFull || target.GetComponent<HarvestTarget>().IsEmpty)
                        {
                            FollowTheLeader();
                        }

                        break;
                    }
                case AntState.following:
                    {
                        if(target == null) target = player;

                        Vector3 followPosition = GetFollowPosition(target); 
                        mover.MoveTo(followPosition);
                        break;
                    }
                default: 
                 {
                     FollowTheLeader();
                     break;
                 }
            }
        }

        private Vector3 GetFollowPosition(GameObject target)    // follows at exactly follow rango to avoid followers making barrier
        {
            Vector3 followDirection = target.transform.position - transform.position;
            followDirection.Normalize();
            return target.transform.position - followDirection * followRange;
        }

        private float GetDistance(GameObject target)
        {
            return Vector3.Distance(target.transform.position,transform.position);
        }

        public void Harvest(GameObject target)
        {
            this.target = target;
            ChangeState(AntState.harvesting);
            harvester.Harvest(target);
        }
        public void Attack(GameObject target)
        {
            this.target = target;
            ChangeState(AntState.attacking);
            fighter.Attack(target);
        }
        public void NotifyNest(PheromoneType pheromoneType)
        {
            target = nest;
            ChangeState(AntState.notifying);
            pheromoneGenerator.StartGeneration(pheromoneType);
            GetComponent<Mover>().MoveTo(target.transform.position);
        }

        public void Store(GameObject target)
        {
            this.target = target;
            ChangeState(AntState.storing);
            harvester.Store(target);
        }

        public void FollowTheLeader()
        {
            target=player;
            currentState = AntState.following;
        }

        private void ChangeState(AntState newState)
        {
            if (newState < currentState)
            {
                GetComponent<ActionScheduler>().CancelCurrentAction();
                currentState = newState;
            }
        }


    }

}