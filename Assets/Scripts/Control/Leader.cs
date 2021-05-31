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
        public int maxFollowers = 5;
        List<Follower> followers;
        public int followerCount {get=>followers.Count;}
        internal bool needsFollower{get => maxFollowers - followers.Count > 0;}

        private void Start()
        {
            followers = new List<Follower>();
        }

        public void AddFollower(GameObject followerObject)  // replaces the worker controller of the worker with follower controller and ads it to the follower list
        {
            if (followerObject == null) return;

            Follower follower = followerObject.GetComponent<Follower>();

            if (follower == null)
            {
                follower = followerObject.AddComponent<Follower>();
            }

            followers.Add(follower);

            follower.GetComponent<Health>().OnDeath += RemoveFollower;
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

        public bool StartPheromoneTrail(int index, PheromoneType type)
        {
            if(index >= followers.Count || index<0) return false;

            followers[index].NotifyNest(type);
            return true;
        }

        public bool CanNotify(int index)
        {
            if (index >= followers.Count || index < 0) return false;

            return followers[index].CanNotify;
        }

    }

}