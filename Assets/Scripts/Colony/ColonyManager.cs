using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using System;
using RPG.Map;
using RPG.Control;

namespace RPG.Colony
{
    [RequireComponent(typeof(Storage))]
    public class ColonyManager : MonoBehaviour
    {
        static ColonyManager instance;
        public Storage storage { get; private set; }

        int maxPopulation = 15;
        List<GameObject> allAnts = new List<GameObject>();
        List<GameObject> availableAnts = new List<GameObject>();
        List<GameObject> buildingAnts = new List<GameObject>();
        List<GameObject> followerAnts = new List<GameObject>();
        GameObject playerAnt;
        Leader leader;
        [SerializeField] GameObject workerPrefab;
        [SerializeField] int startingAnts = 0;

        public void IncreaseMaxStorage(int storageBonus)
        {
            storage.IncreaseMaxCapacity(storageBonus);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (instance != null) Debug.LogWarning("Warning: multiple colonyManager instances active");
            instance = this;

            playerAnt = GameObject.FindGameObjectWithTag("Player");
            if (playerAnt != null) leader = playerAnt.GetComponent<Leader>();
            storage = GetComponent<Storage>();

            for (int i = 0; i < startingAnts; i++)
            {
                CreateWorker();
            }
        }

        private void Update()
        {
            if (leader != null)
            {
                if (leader.needsFollower && availableAnts.Count > 0)
                {
                    GameObject ant = availableAnts[availableAnts.Count - 1];
                    leader.AddFollower(ant);
                    followerAnts.Add(ant);
                    availableAnts.RemoveAt(availableAnts.Count - 1);
                }

            }
        }

        void StartBuildingProcess(int workersNeeded, float releaseTime)   //@todo make this start only when enought ants are waiting on the nest;
        {
            int foundWorkers = 0, i = 0;

            while (foundWorkers < workersNeeded && i < availableAnts.Count)
            {
                GameObject ant = availableAnts[i];
                if (ant.activeSelf == true)
                {
                    DeactivateAntForSeconds(ant, releaseTime);
                    availableAnts.RemoveAt(i);
                    foundWorkers++;
                }
                else
                {
                    i++;
                }
            }

            if (foundWorkers == workersNeeded) return;

            i = 0;
            while (foundWorkers < workersNeeded && i < followerAnts.Count) // if available ants where not enought use also Follower ants
            {
                GameObject ant = followerAnts[i];
                if (ant.activeSelf == true)
                {
                    foundWorkers++;
                    leader.RemoveFollower(ant);
                    DeactivateAntForSeconds(ant, releaseTime);
                    followerAnts.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

        }

        void DeactivateAntForSeconds(GameObject ant, float reactivateTime)
        {
            ant.SetActive(false);
            buildingAnts.Add(ant);

            Invoke("ReactivateWorker", reactivateTime);
        }

        void ReactivateWorker()
        {
            if (buildingAnts.Count != 0)
            {
                GameObject reactivatedAnt = buildingAnts[buildingAnts.Count - 1];
                reactivatedAnt.SetActive(true);
                buildingAnts.RemoveAt(buildingAnts.Count - 1);
                availableAnts.Add(reactivatedAnt);
            }
        }

        private void CreateWorker()
        {
            GameObject newAnt = GameObject.Instantiate(workerPrefab);
            Vector3 spawnPos = transform.position;
            spawnPos.y = MapTools.getTerrainHeight(spawnPos);
            newAnt.transform.position = spawnPos;
            newAnt.transform.parent = transform;
            allAnts.Add(newAnt);
            availableAnts.Add(newAnt);
        }

        public static void ApplyCost(int foodCost, int workerCost, float upgradeTime)
        {
            instance.storage.Consume(foodCost);
            instance.StartBuildingProcess(workerCost, upgradeTime);
        }

        public static bool CheckCost(int foodCost, int workerCost)
        {
            if (foodCost > instance.storage.storedAmount)
            {
                print("not enought food");
                return false;
            }
            if (workerCost > instance.availableAnts.Count + instance.followerAnts.Count)
            {
                print("not enought workers");
                return false;
            }
            return true;
        }

        public static void IncreaseMaxStorage(float bonus)
        {
            instance.storage.IncreaseMaxCapacity(bonus);
        }


        public static void IncreaseMaxPopulation(int bonus)
        {
            instance.maxPopulation += bonus;
        }

        public static void IncreaseMaxFollowers(int bonus)
        {
            instance.leader.maxFollowers+=bonus;
        }

        public static void AddWorker()
        {
            instance.CreateWorker();
        }

        public static void AddPrincess()
        {
            throw new NotImplementedException();
        }


    }
}
