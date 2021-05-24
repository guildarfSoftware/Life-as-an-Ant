using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Pheromones;
using RPG.Core;

namespace RPG.Control
{
    public class Leader : MonoBehaviour
    {
        public int maxFollowers = 2;
        List<Follower> followers;
        internal bool needsFollower{get => maxFollowers - followers.Count > 0;}

        private void Start()
        {
            followers = new List<Follower>();
        }

        GameObject FindCloseWorkerAnt()
        {
            EntityDetector detector = GetComponentInChildren<EntityDetector>();
            if (detector == null) return null;
            var workers = detector.GetEntitiesInLayer(LayerManager.workerLayer);

            foreach (var worker in workers)
            {
                Follower follower = worker.GetComponent<Follower>();
                if (follower == null)
                {
                    return worker;  //need an ant that is not already a follower
                }
            }
            return null;
        }

        public void AddFollower(GameObject followerObject)  // replaces the worker controller of the worker with follower controller and ads it to the follower list
        {
            if (followerObject == null) return;

            WorkerController controller = followerObject.GetComponent<WorkerController>();
            if (controller != null)
            {
                Destroy(controller);
            }

            Follower follower = followerObject.GetComponent<Follower>();

            if (follower == null)
            {
                follower = followerObject.AddComponent<Follower>();
            }

            followers.Add(follower);

            follower.GetComponent<Health>().OnDeath += ()=>{RemoveFollower(followerObject);};
        }

        public void RemoveFollower(GameObject followerObject)
        {
            if (followerObject == null) return;

            Follower follower = followerObject.GetComponent<Follower>();

            if (follower == null) return;

            if (followers.Contains(follower))
            {
                followers.Remove(follower);
            }
        }

        internal void CommandStore(GameObject gameObject)
        {
            foreach (Follower follower in followers)
            {
                follower.Store(gameObject);
            }
        }

        Follower GetFirstFollower()
        {
            if (followers.Count == 0) return null;
            return followers[0];
        }

        public void CommandHarvest(GameObject target)
        {
            foreach (Follower follower in followers)
            {
                follower.Harvest(target);
            }
        }

        public void CommandAttack(GameObject target)
        {
            foreach (Follower follower in followers)
            {
                follower.Attack(target);
            }
        }

        public void FollowMeCommand()
        {
            foreach (Follower follower in followers)
            {
                follower.FollowTheLeader();
            }
        }

        public void CommandNotify(PheromoneType pType, GameObject target)
        {
            foreach (Follower follower in followers)
            {
                if (follower.Notify(pType, target)) return;
            }
        }
    }

}