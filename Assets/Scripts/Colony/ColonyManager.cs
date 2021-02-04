using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using System;
using RPG.Map;
using RPG.Control;
using RPG.Core;

namespace RPG.Colony
{
    [RequireComponent(typeof(Storage))]
    public class ColonyManager : MonoBehaviour
    {
        static ColonyManager instance;
        public Storage storage { get; private set; }

        public float foodRequirement { get => princesses.Count * AntStats.princessFoodConsumption + allAnts.Count * AntStats.workerFoodConsumption; }

        int maxPopulation = 15;
        List<GameObject> allAnts = new List<GameObject>();
        List<GameObject> availableAnts = new List<GameObject>();
        List<GameObject> buildingAnts = new List<GameObject>();
        List<GameObject> followerAnts = new List<GameObject>();
        List<float> princesses = new List<float>(); //list of alive princesses float indicates their health

        GameObject playerAnt;
        Leader leader;
        [SerializeField] GameObject workerPrefab;
        [SerializeField] int startingAnts = 0;

        float timer1Second;

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
            if (playerAnt != null)
            {
                leader = playerAnt.GetComponent<Leader>();
                allAnts.Add(playerAnt);
            } 
            storage = GetComponent<Storage>();
            storage.StoreResource(20);

            for (int i = 0; i < startingAnts; i++)
            {
                CreateWorker();
            }
        }

        private void Update()
        {
            timer1Second += Time.deltaTime;

            if (timer1Second > 1)
            {
                FeedAnts();
                timer1Second = 0;
            }

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

        void FeedAnts()
        {
            float foodConsumed = 0;
            int lowPriorityPrincesses = (int)(princesses.Count * 0.3f); // 30% of princesses will eat last to ensure some workers survive famine
            List<int> markedForRemoval = new List<int>();

            if(storage.storedAmount == 0) 
            {
                playerAnt.GetComponent<Health>().TakeDamage(1);
            }
            else
            {
                foodConsumed += AntStats.workerFoodConsumption;
                playerAnt.GetComponent<Health>().Heal(0.25f); //recover health lost
            }

            if (princesses.Count > 0)
            {
                for (int i = 0; i < princesses.Count - lowPriorityPrincesses; i++)
                {
                    if (foodConsumed < storage.storedAmount)
                    {
                        foodConsumed += AntStats.princessFoodConsumption;
                        princesses[i] += 0.25f; //recover health lost after hunger
                        princesses[i] = Mathf.Min(princesses[i], AntStats.Health);
                    }
                    else
                    {
                        princesses[i] -= 1; //1 damage per second if can't eat;
                        if (princesses[i] <= 0)
                        {
                            markedForRemoval.Add(i);
                        }
                    }
                }
            }

            for (int i = 1; i < allAnts.Count; i++) //start on 1 because player is on position 0 and already fed
            {
                Health antHealth = allAnts[i].GetComponent<Health>();

                if (foodConsumed < storage.storedAmount)
                {
                    foodConsumed += AntStats.workerFoodConsumption;
                    antHealth.Heal(0.25f); //recover health lost after hunger
                }
                else
                {
                    antHealth.TakeDamage(1);//1 damage per second if can't eat;
                }
            }


            for (int i = lowPriorityPrincesses; i < princesses.Count; i++)  //low priority princesses eat last
            {
                if (foodConsumed < storage.storedAmount)
                {
                    foodConsumed += AntStats.princessFoodConsumption;
                    princesses[i] += 0.25f; //recover health lost after hunger
                    princesses[i] = Mathf.Min(princesses[i], AntStats.Health);
                }
                else
                {
                    princesses[i] -= 1; //1 damage per second if can't eat;
                    if (princesses[i] <= 0)
                    {
                        markedForRemoval.Add(i);
                    }
                }
            }
            if (markedForRemoval.Count > 0)
            {
                MessageManager.Message(markedForRemoval.Count.ToString() + " princesses died due to starvation");

                for (int i = markedForRemoval.Count; i >= 0; i--)
                {
                    int indexToRemove = markedForRemoval[i];
                    princesses.RemoveAt(indexToRemove);
                }
            }

            if (foodConsumed > storage.storedAmount) MessageManager.Message("SomeAnts are starving and may die. Gather some food Quick!!");
            storage.Consume(foodConsumed);
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
                MessageManager.Message("Not enought food");
                return false;
            }
            if (workerCost > instance.availableAnts.Count + instance.followerAnts.Count)
            {
                MessageManager.Message("Not enought workers");
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
            instance.leader.maxFollowers += bonus;
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
