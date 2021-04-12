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

        [SerializeField] AntStats workerStats, princesStats;

        public float foodRequirement { get => princesses.Count * princesStats.foodConsumption + allAnts.Count * workerStats.foodConsumption; }

        int maxPopulation = 15;
        public int MaxPopulation { get => maxPopulation; }
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
        internal Action onPopulationChange;

        public int AvailableAntsCount { get => availableAnts.Count; }
        public int currentPopulation { get => allAnts.Count; }
        public int princessPopulation { get => princesses.Count; }
        public void IncreaseMaxStorage(int storageBonus)
        {
            storage.IncreaseMaxCapacity(storageBonus);
        }

        // Start is called before the first frame update
        void Awake()
        {
            if (instance != null) Debug.LogWarning("Warning: multiple colonyManager instances active");
            instance = this;

            playerAnt = GameObject.FindGameObjectWithTag("Player");
            if (playerAnt != null)
            {
                playerAnt.GetComponent<Health>().OnDeath += ()=>{Invoke("RespawnPlayer",1f);};
                leader = playerAnt.GetComponent<Leader>();
                allAnts.Add(playerAnt);
            }
            storage = GetComponent<Storage>();
            storage.StoreResource(20);

            for (int i = 0; i < startingAnts; i++)
            {
                CreateWorker();
            }

            onPopulationChange?.Invoke();
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
                    onPopulationChange?.Invoke();
                }

            }
        }

        void RespawnPlayer()
        {
            if (currentPopulation > 1)  //there is at least 1 ant apart from the player
            {
                MessageManager.Message("You Died but you can now control another ant of the colony");
                GameObject substitute = allAnts[allAnts.Count-1];
                
                playerAnt.GetComponent<PlayerController>().Respawn(substitute);

                substitute.GetComponent<Health>().OnDeath();
                Destroy(substitute);


                return;
            }

            MessageManager.Message("Game Over");

        }

        void FeedAnts()
        {
            float foodConsumed = 0;
            int lowPriorityPrincesses = (int)(princesses.Count * 0.3f); // 30% of princesses will eat last to ensure some workers survive famine
            List<int> markedForRemoval = new List<int>();

            if (storage.StoredAmount == 0)
            {
                playerAnt.GetComponent<Health>().TakeDamage(1);
            }
            else
            {
                foodConsumed += workerStats.foodConsumption;
                playerAnt.GetComponent<Health>().Heal(0.25f); //recover health lost
            }

            if (princesses.Count > 0)
            {
                for (int i = 0; i < princesses.Count - lowPriorityPrincesses; i++)
                {
                    if (foodConsumed < storage.StoredAmount)
                    {
                        foodConsumed += princesStats.foodConsumption;
                        princesses[i] += 0.25f; //recover health lost after hunger
                        princesses[i] = Mathf.Min(princesses[i], princesStats.Health);
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

                if (foodConsumed < storage.StoredAmount)
                {
                    foodConsumed += workerStats.foodConsumption;
                    antHealth.Heal(0.25f); //recover health lost after hunger
                }
                else
                {
                    antHealth.TakeDamage(1);//1 damage per second if can't eat;
                }
            }


            for (int i = lowPriorityPrincesses; i < princesses.Count; i++)  //low priority princesses eat last
            {
                if (foodConsumed < storage.StoredAmount)
                {
                    foodConsumed += princesStats.foodConsumption;
                    princesses[i] += 0.25f; //recover health lost after hunger
                    princesses[i] = Mathf.Min(princesses[i], princesStats.Health);
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

            if (foodConsumed > storage.StoredAmount) MessageManager.Message("SomeAnts are starving and may die. Gather some food Quick!!");
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
            onPopulationChange?.Invoke();

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
                onPopulationChange?.Invoke();
            }
        }

        private void CreateWorker()
        {
            GameObject newAnt = GameObject.Instantiate(workerPrefab);
            Vector3 spawnPos = transform.position;
            spawnPos.y = MapTools.getTerrainHeight(spawnPos);
            newAnt.transform.position = spawnPos;
            newAnt.transform.parent = transform;

            newAnt.GetComponent<StatsManager>().values = workerStats;

            newAnt.GetComponent<Health>().OnDeath += () => { RemoveAnt(newAnt); };

            allAnts.Add(newAnt);
            availableAnts.Add(newAnt);
            onPopulationChange?.Invoke();
        }

        void RemoveAnt(GameObject ant)
        {
            if (allAnts.Contains(ant)) allAnts.Remove(ant);
            if (availableAnts.Contains(ant)) availableAnts.Remove(ant);
            if (buildingAnts.Contains(ant)) buildingAnts.Remove(ant);
            if (followerAnts.Contains(ant)) followerAnts.Remove(ant);
            onPopulationChange?.Invoke();
        }

        public static void ApplyCost(int foodCost, int workerCost, float upgradeTime)
        {
            instance.storage.Consume(foodCost);
            instance.StartBuildingProcess(workerCost, upgradeTime);
        }

        public static bool CheckCost(int foodCost, int workerCost)
        {
            if (foodCost > instance.storage.StoredAmount)
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
            instance.onPopulationChange?.Invoke();
        }

        public static void IncreaseMaxFollowers(int bonus)
        {
            instance.leader.maxFollowers += bonus;
        }

        public static void AddWorker()
        {
            if (instance.allAnts.Count >= instance.maxPopulation)
            {
                MessageManager.Message("Reached Max population. Upgrade population limit to keep growing");
                return;
            }

            instance.CreateWorker();
        }

        public static void AddPrincess()
        {
            instance.princesses.Add(instance.princesStats.Health);
            instance.onPopulationChange?.Invoke();
        }

        internal static void IncreaseMaxHealth(float bonus)
        {
            instance.workerStats.healthBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.healthBonus += bonus;
        }

        internal static void IncreaseMaxDamage(float bonus)
        {
            instance.workerStats.damageBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.damageBonus += bonus;
        }

        internal static void IncreaseMaxSpeed(float bonus)
        {
            instance.workerStats.speedBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.speedBonus += bonus;
        }

        internal static void IncreaseCarryCapacity(float bonus)
        {
            instance.workerStats.carryCapacityBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.carryCapacityBonus += bonus;
        }
    }
}
