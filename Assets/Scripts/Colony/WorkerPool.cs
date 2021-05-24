using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using UnityEngine;

public class WorkerPool
{
    static GameObject prefabWorker;

    static List<GameObject> pool;

    const int initialCapacity = 6;


    public static void Initialize()
    {
        prefabWorker = Resources.Load<GameObject>("WorkerAnt");

        pool = new List<GameObject>(initialCapacity);

        for (int i = 0; i < pool.Capacity; i++)
        {
            pool.Add(GetNewWorker());
        }


    }

    static GameObject GetNewWorker()
    {
        GameObject obj;

        obj = GameObject.Instantiate(prefabWorker);
        obj.SetActive(false);

        return obj;
    }

    public static GameObject GetWorker()
    {
        if (pool.Count > 0)
        {
            GameObject returned = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);

            returned.GetComponent<WorkerController>().Initialize();
            return returned;
        }
        else
        {
            GameObject returned = GetNewWorker();
            pool.Capacity++;
            return returned;
        }
    }

    public static void ReturnWorker(GameObject worker)
    {
        Follower follower = worker.GetComponent<Follower>();
        if (follower != null)
        {
            GameObject.Destroy(follower);
        }

        WorkerController controller = worker.GetComponent<WorkerController>();
        if (controller == null)
        {
            controller = worker.AddComponent<WorkerController>();
        }
        
        worker.SetActive(false);
        pool.Add(worker);
    }



    public static void EmptyPool()
    {
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            pool.RemoveAt(i);
        }
    }
}
