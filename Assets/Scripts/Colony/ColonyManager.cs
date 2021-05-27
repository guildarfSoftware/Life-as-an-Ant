using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using System;
using RPG.Map;
using RPG.Control;
using RPG.Core;
using MyTools;

namespace RPG.Colony
{
    [RequireComponent(typeof(Storage))]
    public class ColonyManager : MonoBehaviour
    {
        #region variables
        static ColonyManager instance;

        [SerializeField] AntStats workerStats;
        [SerializeField] GameObject workerPrefab;
        [SerializeField] int startingAnts = 0;

        public Action onPopulationChange;
        public Storage storage { get; private set; }
        public int MaxPopulation { get => maxPopulation; }
        public int currentPopulation { get => AvailableAnts + buildingAntsCount; }
        public int AvailableAnts { get => AvailableWorkers + followerAnts.Count + 1; }
        public int AvailableWorkers { get => availableAnts.Count + restingAnts - buildingAntsRequired; }
        public int buildingAntsCount { get => buildingAntsCollected + buildingAntsRequired; }
        public float foodRequirement { get => currentPopulation * workerStats.foodConsumption; }


        int maxPopulation = 15;
        List<GameObject> allAnts = new List<GameObject>();
        List<GameObject> availableAnts = new List<GameObject>();
        List<GameObject> followerAnts = new List<GameObject>();
        int buildingAntsRequired;
        int buildingAntsCollected;
        int restingAnts;

        GameObject playerAnt;
        Leader leader;

        float oneSecondCounter;
        int timeWithoutFood;
        float restTime = 10f;
        #endregion

        #region UnityMethods
        void Awake()
        {
            if (instance != null) Debug.LogWarning("Warning: multiple colonyManager instances active");
            instance = this;

            playerAnt = GameObject.FindGameObjectWithTag("Player");
            if (playerAnt != null)
            {
                playerAnt.GetComponent<Health>().OnDeath += () => { Invoke("RespawnPlayer", 1f); };
                leader = playerAnt.GetComponent<Leader>();
                allAnts.Add(playerAnt);
            }
            storage = GetComponent<Storage>();

            WorkerPool.Initialize(this.gameObject);

            for (int i = 0; i < startingAnts; i++)
            {
                CreateWorker();
            }

            onPopulationChange?.Invoke();
        }

        private void Update()
        {
            oneSecondCounter += Time.deltaTime;

            if (oneSecondCounter > 1)
            {
                FeedAnts();
                oneSecondCounter = 0;
            }
        }
        #endregion

        void FeedAnts()
        {
            float foodConsumed = foodRequirement;
            storage.Consume(foodConsumed);

            if (storage.StoredAmount == 0)
            {
                if (timeWithoutFood == 0)
                {
                    MessageManager.Message("Carefull", "Some ants are starving and may die. Gather some food Quick!!", null, null);
                }
                timeWithoutFood++;
            }
            else
            {
                timeWithoutFood = 0;
            }

            if (timeWithoutFood > workerStats.Health)
            {
                if (restingAnts != 0)
                {
                    restingAnts--;
                }
                else if (buildingAntsCollected != 0)
                {
                    buildingAntsCollected--;
                }
                else
                {
                    int lastAntIndex = allAnts.Count - 1;
                    Health lastAntHealth = allAnts[lastAntIndex].GetComponent<Health>();
                    if (lastAntIndex != 0)
                    {
                        lastAntHealth.TakeDamage(lastAntHealth.currentHealth);
                    }
                    else
                    {
                        lastAntHealth.TakeDamage(1);
                    }
                }

            }

        }

        void StartBuildingProcess(int workersNeeded, float releaseTime)
        {
            if (workersNeeded > AvailableWorkers) return;

            int newWorkersNeeded;

            if (restingAnts > workersNeeded)
            {
                newWorkersNeeded = 0;

                buildingAntsCollected += workersNeeded;
                restingAnts -= workersNeeded;
            }
            else
            {
                newWorkersNeeded = workersNeeded - restingAnts;

                buildingAntsCollected += restingAnts;
                restingAnts = 0;
            }

            buildingAntsRequired += newWorkersNeeded;
            Tools.SortByDistance(gameObject, ref availableAnts);

            for (int i = 0; i < newWorkersNeeded; i++)
            {
                GameObject ant = availableAnts[i];
                WorkerController workerMind = ant.GetComponent<WorkerController>();
                workerMind.ReturnToBuild();
            }

            StartCoroutine(ReactivateBuilders(releaseTime, workersNeeded));
            onPopulationChange?.Invoke();

        }

        void OnEnterAnthill(bool isBuilder, GameObject antObject)
        {
            if (isBuilder)
            {
                buildingAntsCollected++;
                buildingAntsRequired--;
            }
            else
            {
                restingAnts++;
                StartCoroutine(reactivateRestingAnt(restTime));
            }
            RemoveAnt(antObject);
        }

        IEnumerator ReactivateBuilders(float releaseTime, int totalWorkers)
        {
            yield return new WaitForSeconds(releaseTime);

            int workersReactivated = Mathf.Min(totalWorkers, buildingAntsCollected);
            int remainingOrLost = totalWorkers - workersReactivated;

            for (int i = 0; i < workersReactivated; i++)
            {
                CreateWorker();
            }

            buildingAntsRequired -= remainingOrLost;
            buildingAntsCollected -= workersReactivated;
            onPopulationChange?.Invoke();
        }

        IEnumerator reactivateRestingAnt(float restTime)
        {
            yield return new WaitForSeconds(restTime);

            if (restingAnts > 0)
            {
                restingAnts--;
                CreateWorker();
            }
        }

        void RespawnPlayer()
        {
            if (currentPopulation > 1)  //there is at least 1 ant apart from the player
            {
                MessageManager.Message("Oops", "You Died but you can now control another ant of the colony", null, null);
                GameObject substitute = allAnts[allAnts.Count - 1];

                playerAnt.GetComponent<PlayerController>().Respawn(substitute.transform.position);

                RemoveAnt(substitute);


                return;
            }

            MessageManager.Message("Game Over", " You died", null, null);

        }

        void CreateWorker()
        {
            GameObject newAnt = WorkerPool.GetWorker();
            newAnt.transform.SetParent(transform, true);

            newAnt.GetComponent<WorkerController>().EnterAnthill += OnEnterAnthill;

            Vector3 spawnPos = transform.position;
            spawnPos.y = MapTools.getTerrainHeight(spawnPos);
            newAnt.transform.position = spawnPos;

            newAnt.GetComponent<StatsManager>().values = workerStats;

            newAnt.GetComponent<Health>().OnDeath += () => { RemoveAnt(newAnt); };

            newAnt.SetActive(true);

            allAnts.Add(newAnt);
            availableAnts.Add(newAnt);
            onPopulationChange?.Invoke();
        }

        void RemoveAnt(GameObject ant)
        {
            if (allAnts.Contains(ant)) allAnts.Remove(ant);
            if (availableAnts.Contains(ant)) availableAnts.Remove(ant);
            if (followerAnts.Contains(ant)) followerAnts.Remove(ant);
            ant.GetComponent<WorkerController>().EnterAnthill -= OnEnterAnthill;
            WorkerPool.ReturnWorker(ant);
            onPopulationChange?.Invoke();
        }

        public static void ApplyCost(int foodCost, int workerCost, float upgradeTime)
        {
            instance.storage.Consume(foodCost);
            if (upgradeTime != 0) instance.StartBuildingProcess(workerCost, upgradeTime);
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
            if (instance.currentPopulation >= instance.maxPopulation)
            {
                MessageManager.Message("Ooops", "Reached Max population. Upgrade population limit to keep growing", null, null);
                return;
            }

            instance.CreateWorker();
        }

        public static void IncreaseMaxHealth(float bonus)
        {
            instance.workerStats.healthBonus += bonus;
            AntStats playerStats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            playerStats.healthBonus += bonus;

            foreach (GameObject ant in instance.allAnts)
            {
                ant.GetComponent<Health>().Heal(bonus);
            }
        }

        public static void IncreaseMaxDamage(float bonus)
        {
            instance.workerStats.damageBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.damageBonus += bonus;
        }

        public static void IncreaseMaxSpeed(float bonus)
        {
            instance.workerStats.speedBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.speedBonus += bonus;
        }

        public static void IncreaseCarryCapacity(float bonus)
        {
            instance.workerStats.carryCapacityBonus += bonus;
            AntStats stats = (AntStats)instance.playerAnt.GetComponent<StatsManager>().values;
            stats.carryCapacityBonus += bonus;
        }

        public static float GetStoredFood()
        {
            return instance.storage.StoredAmount;
        }

        public static int GetAvailableWorkersCount()
        {
            return instance.AvailableWorkers;
        }

        public static int GetFollowerCount()
        {
            return instance.followerAnts.Count;
        }

        public void IncreaseMaxStorage(int storageBonus)
        {
            storage.IncreaseMaxCapacity(storageBonus);
        }

    }
}
