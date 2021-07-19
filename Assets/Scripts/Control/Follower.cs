using System;
using System.Collections;
using System.Collections.Generic;
using MyTools;
using RPG.Combat;
using RPG.Core;
using RPG.Harvest;
using RPG.Movement;
using RPG.Pheromones;
using RPG.Sounds;
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

        float nestRange = 6f;
        PheromoneType pheromoneType = PheromoneType.None;
        bool isNotifying;
        public bool CanNotify { get => !isNotifying; }



        private void Awake()
        {
            health = GetComponent<Health>();
            health.OnDeath += CreateCorpse;
            pheromoneGenerator = GetComponent<PheromoneGenerator>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            fighter.EnterCombat += harvester.DropFood;
            mover = GetComponent<Mover>();

            nest = GameObject.FindGameObjectWithTag("Nest");

            leader = GameObject.FindGameObjectWithTag("Player");
            detector = leader.GetComponentInChildren<EntityDetector>();
            leader.GetComponent<Harvester>().fooodGrabbed += HarvestCloseFood;
            leader.GetComponent<Fighter>().EnterCombat += AttackCloseEnemy;

            StatsManager followerStats = GetComponent<StatsManager>();
            StatsManager leaderStats = leader.GetComponent<StatsManager>();
            followerStats.values = leaderStats.values;

            SetStateMachineCallbacks();


            stateMachine.State = Following;
        }


        private void Update()
        {
            stateMachine.Update();
        }

        private void OnEnable()
        {
            if (health.IsDead) health.Revive();
            stateMachine.State = Following;
        }

        #region StateMAchineMethods

        void SetStateMachineCallbacks()
        {
            stateMachine = new StateMachine(StateCount);
            stateMachine.SetCallbacks(Following, FollowingUpdate, null, FollowingStart, null);
            stateMachine.SetCallbacks(Harvesting, HarvestingUpdate, null, null, HarvestingExit);
            stateMachine.SetCallbacks(Attacking, AttackingUpdate, null, null, AttackingExit);
            stateMachine.SetCallbacks(Storing, StoringUpdate, null, null, null);
            stateMachine.SetCallbacks(Notifying, NotifyingUpdate, null, NotifyingStart, null);
        }

        void FollowingStart()
        {
            pheromoneType = PheromoneType.None;
            harvestTarget = null;
            attackTarget = null;
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        int FollowingUpdate()
        {
            if (pheromoneType != PheromoneType.None)
            {
                return Notifying;
            }
            if (harvester.CanStore(nest) && Tools.GetDistance(nest, gameObject) < nestRange)
            {
                harvester.Store(nest);
                return Storing;
            }
            if (attackTarget != null)
            {
                return Attacking;
            }
            if (harvestTarget != null)
            {
                return Harvesting;
            }


            Vector3 followPosition = GetFollowPosition(leader);

            if (Tools.GetDistance(followPosition, transform.position) > 0.5f)
            {
                mover.MoveTo(followPosition);
            }
            else
            {
                mover.Cancel();
                if(isNotifying)
                {
                    SoundEffects.PlaySound(ClipId.FollowerReturned);
                }
                isNotifying = false;
            }
            return Following;
        }

        int HarvestingUpdate()
        {
            if (pheromoneType != PheromoneType.None)
            {
                return Notifying;
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
            return Following;
        }

        void HarvestingExit()
        {
            harvestTarget = null;
            harvester.Cancel();
        }

        int AttackingUpdate()
        {
            if (pheromoneType != PheromoneType.None)
            {
                return Notifying;
            }
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
            return Following;
        }
        void AttackingExit()
        {
            attackTarget = null;
            fighter.Cancel();
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
            isNotifying = true;
            pheromoneGenerator.StartGeneration(pheromoneType);
            mover.MoveTo(nest.transform.position);
        }


        int NotifyingUpdate()
        {
            mover.MoveTo(nest.transform.position);
            if (Tools.GetDistance(nest, gameObject) < nestRange)
            {
                pheromoneGenerator.StopGeneration(nest.transform);
                pheromoneType = PheromoneType.None;
                return Following;
            }

            return Notifying;
        }
        #endregion



        private void CreateCorpse(GameObject gObject)
        {
            Transform bodyTransform = transform.GetChild(0);
            GameObject corpse = Instantiate(bodyTransform.gameObject, bodyTransform.position, bodyTransform.rotation);
            GameObject.Destroy(corpse, 30);
            transform.GetChild(0).localRotation = Quaternion.Euler(0, -90, 0);
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

        }

        private void HarvestCloseFood()
        {
            harvestTarget = detector.GetClosestEntityInLayer(LayerManager.foodLayer);
        }

        public void NotifyNest(PheromoneType type)
        {
            pheromoneType = type;
        }

    }

}