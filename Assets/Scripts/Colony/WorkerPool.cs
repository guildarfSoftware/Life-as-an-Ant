using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using UnityEngine;
using RPG.MyTools;

namespace RPG.Colony
{
    public static class WorkerPool
    {
        static Pool workerPool;
        public static void Initialize()
        {
            GameObject prefabWorker = UnityEngine.Resources.Load<GameObject>("WorkerAnt");
            Transform parent = ColonyManager.instance.transform;
            workerPool = new Pool(prefabWorker, parent, "Worker");
        }
        public static GameObject GetWorker()
        {
            GameObject worker = workerPool.GetPooledObject();
            return worker;
        }

        public static void ReturnWorker(GameObject worker)
        {
            workerPool.ReturnObject(worker);
        }



        public static void EmptyPool()
        {
            workerPool.EmptyPool();
        }
    }

    public static class FollowerPool
    {
        static Pool followerPool;
        public static void Initialize()
        {
            GameObject prefabFollower = UnityEngine.Resources.Load<GameObject>("FollowerAnt");
            Transform parent = ColonyManager.instance.transform;
            followerPool = new Pool(prefabFollower, parent, "Follower");
        }
        public static GameObject GetFollower()
        {
            GameObject follower = followerPool.GetPooledObject();
            return follower;
        }

        public static void ReturnFollower(GameObject follower)
        {
            followerPool.ReturnObject(follower);
        }

        public static void EmptyPool()
        {
            followerPool.EmptyPool();
        }
    }
}
