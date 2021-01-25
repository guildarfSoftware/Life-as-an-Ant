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

        Follower notifier;    //will be in charge of sending pheromone signals to the nest


        private void Start()
        {
            followers = new List<Follower>();
        }

        private void Update()
        {
            if (followers.Count < maxFollowers)
            {
                GameObject newFollower = FindCloseWorkerAnt();
                if (newFollower != null)
                {
                    AddFollower(newFollower);
                }
            }
        }

        GameObject FindCloseWorkerAnt()
        {
            EntityDetector detector = GetComponent<EntityDetector>();
            if (detector == null) return null;
            var workers = detector.GetEntitiesWithTag("Worker");

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
            if (notifier == null) notifier = follower;
        }

        public void RemoveFollower(GameObject followerObject)
        {
            if (followerObject == null) return;

            Follower follower = followerObject.GetComponent<Follower>();

            if (follower == null) return;

            if (followers.Contains(follower))
            {
                followers.Remove(follower);
                if (follower == notifier)
                {
                    notifier = GetFirstFollower();  //replaces the notifier or sets it null if no other follower available
                }
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

        public void CommandHarvest(GameObject gameObject)
        {
            foreach (Follower follower in followers)
            {
                if (follower == notifier)
                {
                    follower.NotifyNest(PheromoneType.Harvest);
                }
                else
                {
                    follower.Harvest(gameObject);
                }
            }
        }

        public void CommandAttack(GameObject gameObject)
        {
            foreach (Follower follower in followers)
            {
                if (follower == notifier)
                {
                    follower.NotifyNest(PheromoneType.Combat);
                }
                else
                {
                    follower.Attack(gameObject);
                }
            }
        }

        public void FollowMeCommand()
        {
            foreach (Follower follower in followers)
            {
                if (follower != notifier)   //notify to nest cannot be overwritten
                {
                    follower.FollowTheLeader();
                }
            }
        }
    }

}