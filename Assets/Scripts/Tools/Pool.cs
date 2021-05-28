using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.MyTools
{
    public class Pool
    {
        GameObject prefab;
        string objectName;
        Transform parent;

        int created;

        List<GameObject> pool;


        const int initialCapacity = 6;


        public Pool(GameObject prefab, Transform parent = null, string objectName = "object")
        {
            this.prefab = prefab;
            this.parent = parent;
            this.objectName = objectName;

            pool = new List<GameObject>(initialCapacity);
            for (int i = 0; i < pool.Capacity; i++)
            {
                GameObject newObject = GetNewObject();
                pool.Add(newObject);
            }


        }

        GameObject GetNewObject()
        {
            GameObject obj;

            obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);

            obj.name = objectName + created;
            created++;

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        public GameObject GetPooledObject()
        {
            GameObject returned;
            
            if (pool.Count > 0)
            {
                returned = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);

            }
            else
            {
                returned = GetNewObject();
                pool.Capacity++;
                
            }
            returned.SetActive(true);
            return returned;
        }

        public void ReturnObject(GameObject returnedObject)
        {
            returnedObject.SetActive(false);
            pool.Add(returnedObject);
        }



        public void EmptyPool()
        {
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                pool.RemoveAt(i);
            }
        }
    }
}
