using System;
using System.Collections;
using System.Collections.Generic;
using MyTools;
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

        StateMachine stateMachine;

        enum followerStates
        {
            following = 0,
            attacking,
            harvesting,
            storing,
            notifying,
            Count,
        }
        int Following { get => (int)followerStates.following; }
        int Attacking { get => (int)followerStates.attacking; }
        int Harvesting { get => (int)followerStates.harvesting; }
        int Storing { get => (int)followerStates.storing; }
        int Notifying { get => (int)followerStates.notifying; }
        int StateCount { get => (int)followerStates.Count; }

        Health health;
        EntityDetector detector;
        Harvester harvester;
        Fighter fighter;
        PheromoneGenerator pheromoneGenerator;
        Mover mover;
        GameObject nest, leader;
        float followRange = 5f;
        private float maxPlayerDistance = 8f;

        GameObject harvestTarget;
        GameObject attackTarget;

        bool isNotifying;
        bool isFollowing;
        private float nestRange = 3f;

        private void Start()
        {
            health = GetComponent<Health>();
            pheromoneGenerator = GetComponent<PheromoneGenerator>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();

            nest = GameObject.FindGameObjectWithTag("Nest");

            leader = GameObject.FindGameObjectWithTag("Player");
            detector = leader.GetComponentInChildren<EntityDetector>();
            leader.GetComponent<Harvester>().fooodGrabbed += HarvestCloseFood;
            leader.GetComponent<Fighter>().InCombat += AttackCloseEnemy;

            StatsManager followerStats = GetComponent<StatsManager>();
            StatsManager leaderStats = leader.GetComponent<StatsManager>();
            followerStats.values = leaderStats.values;

            stateMachine = new StateMachine(StateCount);
            stateMachine.SetCallbacks(Following, FollowingUpdate, null, null, null);
            stateMachine.SetCallbacks(Harvesting, HarvestingUpdate, null, null, null);
            stateMachine.SetCallbacks(Attacking, AttackingUpdate, null, null, null);
            stateMachine.SetCallbacks(Storing, StoringUpdate, null, null, null);
            stateMachine.SetCallbacks(Notifying, NotifyingUpdate, null, NotifyingStart, null);

            stateMachine.State = Following;
        }

        #region StateMAchineMethods
        int FollowingUpdate()
        {

            if (attackTarget != null)
            {
                return Attacking;
            }
            if (harvestTarget != null)
            {
                return Harvesting;
            }

            if (harvester.CanStore(nest) && detector.GetEntityInLayer(LayerManager.anthillLayer) != null)
            {
                harvester.Store(nest);
                return Storing;
            }

            Vector3 followPosition = GetFollowPosition(leader);
            mover.MoveTo(followPosition);
            return Following;
        }

        int HarvestingUpdate()
        {
            if (Tools.GetDistance(leader, gameObject) > maxPlayerDistance)
            {
                harvester.Cancel();
                return Following;
            }
            if (attackTarget != null)
            {
                return Attacking;
            }
            if (harvestTarget != null && harvester.CanHarvest(harvestTarget))
            {
                harvester.Harvest(harvestTarget);
                return Harvesting;
            }
            harvestTarget = null;
            return Following;
        }

        int AttackingUpdate()
        {
            if (Tools.GetDistance(leader, gameObject) > maxPlayerDistance)
            {
                fighter.Cancel();
                attackTarget = null;
                return Following;
            }
            if (attackTarget != null && fighter.CanAttack(attackTarget))
            {
                fighter.Attack(attackTarget);
                return Attacking;
            }

            if (detector.GetEntitiesInLayer(LayerManager.enemyLayer).Count != 0)
            {
                AttackCloseEnemy();
                return Attacking;
            }

            return Following;
        }

        int StoringUpdate()
        {
            if (harvester.CanHarvest(nest))
            {
                harvester.Store(nest);
                return Storing;
            }

            return Following;

        }

        void NotifyingStart()
        {
            pheromoneGenerator.StartGeneration(PheromoneType.Combat);
            mover.MoveTo(nest.transform.position);
        }


        int NotifyingUpdate()
        {
            mover.MoveTo(nest.transform.position);
            if (Tools.GetDistance(nest, gameObject) < nestRange)
            {
                pheromoneGenerator.StopGeneration(nest.transform);
                return Following;
            }
            return Notifying;
        }
        #endregion


        private void Update()
        {
            stateMachine.Update();
        }


        private Vector3 GetFollowPosition(GameObject target)    // follows at exactly follow rang to avoid followers making barrier
        {
            Vector3 followDirection = target.transform.position - transform.position;
            followDirection.Normalize();
            return target.transform.position - followDirection * followRange;
        }



        private void AttackCloseEnemy()
        {
            attackTarget = detector.GetClosestEntityInLayer(LayerManager.enemyLayer);
            if(attackTarget!= null)
            {
                fighter.Attack(attackTarget);
            }
            isFollowing = false;
        }

        private void HarvestCloseFood()
        {
            harvestTarget = detector.GetClosestEntityInLayer(LayerManager.foodLayer);
            harvester.Harvest(harvestTarget);
            isFollowing = false;
        }


    }

}