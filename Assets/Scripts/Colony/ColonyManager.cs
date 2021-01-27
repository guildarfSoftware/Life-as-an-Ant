﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using System;

namespace RPG.Colony
{
    [RequireComponent(typeof(Storage))]
    public class ColonyManager : MonoBehaviour
    {
        public Storage storage { get; private set; }
        List<GameObject> ants =  new List<GameObject>();

        [SerializeField] GameObject workerPrefab;
        [SerializeField] int startingAnts=0;
        // Start is called before the first frame update
        void Start()
        {
            storage = GetComponent<Storage>();
            for (int i = 0; i < startingAnts; i++)
            {
                CreateWorker();
            }
        }

        public bool BuyWorker()
        {
            if (storage.storedAmount < 30) return false;
            CreateWorker();
            storage.Consume(30);

            return true;
        }

        private void CreateWorker()
        {
            GameObject newAnt = GameObject.Instantiate(workerPrefab);
            newAnt.transform.position = transform.position;
            newAnt.transform.parent = transform;
            ants.Add(newAnt);
        }
    }
}
