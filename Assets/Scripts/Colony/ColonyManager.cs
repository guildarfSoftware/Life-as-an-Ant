using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using System;
using RPG.Map;
using RPG.Control;
using RPG.Core;
using MyTools;
using RPG.MyTools;

namespace RPG.Colony
{
    [RequireComponent(typeof(Storage))]
    public class ColonyManager : MonoBehaviour
    {
        #region variables
        static public ColonyManager instance;

        [SerializeField] AntStats workerStats;
        [SerializeField] GameObject workerPrefab;
        [SerializeField] int startingAnts = 0;

        public Action onPopulationChange;
        public Storage storage { get; private set; }
        public int MaxPopulation { get => maxPopulation; }
        public int FollowerAnts { get => followersList.Count; }
        public int AvailableWorkers { get => workersList.Count + restingAnts - buildingAntsRemaining; }
        public int AvailableAnts { get => AvailableWorkers + FollowerAnts + 1; }
        public int BuildingAnts { get => buildingAntsCollected + buildingAntsRemaining; }
        public int currentPopulation { get => AvailableAnts + BuildingAnts; }
        public float FoodRequirement { get => currentPopulation * workerStats.foodConsumption; }
        public float FoodAvailable { get => storage.StoredAmount; }


        int maxPopulation = 15;
        List<GameObject> allAntsList = new List<GameObject>();
        List<GameObject> workersList = new List<GameObject>();
        List<GameObject> followersList = new List<GameObject>();
        int buildingAntsRemaining;
        int buildingAntsCollected;
        int restingAnts;
        Pool workerPool;

        GameObject playerAnt;
        Leader leader;

        float oneSecondCounter;
        int timeWithoutFood;
        float antRestTime = 5f;
        #endregion

        #region UnityMethods
        void Awake()
        {
            if (instance != null) Debug.LogWarning("Warning: multiple colonyManager instances active");
            instance = this;

            playerAnt = GameObject.FindGameObjectWithTag("Player");
            if (playerAnt != null)
            {
                playerAnt.GetComponent<Health>().OnDeath += OnPlayerDeath;
                leader = playerAnt.GetComponent<Leader>();
                allAntsList.Add(playerAnt);
            }
            storage = GetComponent<Storage>();

            WorkerPool.Initialize();

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

        #region Listeners
        void OnEnterAnthill(bool isBuilder, GameObject antObject)
        {
            if (isBuilder)
            {
                buildingAntsCollected++;
                buildingAntsRemaining--;
            }
            else
            {
                restingAnts++;
                StartCoroutine(reactivateRestingAnt(antRestTime));
            }
            RemoveAnt(antObject);
        }

        void OnPlayerDeath(GameObject gObject)
        {
            if (currentPopulation > 1)  //there is at least 1 ant apart from the player
            {
                MessageManager.Message("Oops", "You Died, but you can now control another ant of the colony", null, null);
                GameObject substitute = allAntsList[allAntsList.Count - 1];

                playerAnt.GetComponent<PlayerController>().Respawn(substitute.transform.position);

                RemoveAnt(substitute);

                return;
            }


            MessageManager.Message("Game Over", " You died", RPG.UI.UIManager.LoadMenuScene, null);

        }

        void OnWorkerDeath(GameObject worker)
        {
            RemoveAnt(worker);
        }
        #endregion

        #region private
        void FeedAnts()
        {
            float foodConsumed = FoodRequirement;
            storage.Consume(foodConsumed);

            if (FoodAvailable <= 0)
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
                    int lastAntIndex = allAntsList.Count - 1;
                    Health lastAntHealth = allAntsList[lastAntIndex].GetComponent<Health>();
                    if (lastAntIndex != 0)
                    {
                        lastAntHealth.TakeDamage(lastAntHealth.currentHealth);
                    }
                    else
                    {
                        lastAntHealth.TakeDamage(1);
                    }
                }
                onPopulationChange?.Invoke();

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

            buildingAntsRemaining += newWorkersNeeded;
            Tools.SortByDistance(gameObject, ref workersList);

            for (int i = 0; i < newWorkersNeeded; i++)
            {
                GameObject ant = workersList[i];
                WorkerController workerMind = ant.GetComponent<WorkerController>();
                workerMind.ReturnToBuild();
            }

            StartCoroutine(ReactivateBuilders(releaseTime, workersNeeded));
            onPopulationChange?.Invoke();

        }


        IEnumerator ReactivateBuilders(float buildTime, int totalWorkers)
        {
            yield return new WaitForSeconds(buildTime);

            int workersReactivated = Mathf.Min(totalWorkers, buildingAntsCollected);
            int remainingOrLost = totalWorkers - workersReactivated;

            for (int i = 0; i < workersReactivated; i++)
            {
                CreateWorker();
            }

            buildingAntsRemaining -= remainingOrLost;
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

        void CreateWorker()
        {
            GameObject newAnt = WorkerPool.GetWorker();

            newAnt.GetComponent<WorkerController>().EnterAnthill += OnEnterAnthill;

            Vector3 spawnPos = transform.position;
            spawnPos.y = MapTools.getTerrainHeight(spawnPos);
            newAnt.transform.position = spawnPos;

            newAnt.GetComponent<StatsManager>().values = workerStats;

            newAnt.GetComponent<Health>().OnDeath += OnWorkerDeath;

            newAnt.SetActive(true);

            allAntsList.Add(newAnt);
            workersList.Add(newAnt);
            onPopulationChange?.Invoke();
        }

        void RemoveAnt(GameObject ant)
        {
            if (allAntsList.Contains(ant)) allAntsList.Remove(ant);
            if (followersList.Contains(ant)) followersList.Remove(ant);

            if (workersList.Contains(ant))
            {
                ant.GetComponent<WorkerController>().EnterAnthill -= OnEnterAnthill;
                ant.GetComponent<Health>().OnDeath -= OnWorkerDeath;
                workersList.Remove(ant);
            }

            WorkerPool.ReturnWorker(ant);
            onPopulationChange?.Invoke();
        }
        #endregion

        #region public methods
        public void ApplyCost(int foodCost, int workerCost, float upgradeTime)
        {
            storage.Consume(foodCost);
            if (upgradeTime != 0) StartBuildingProcess(workerCost, upgradeTime);
        }


        public void IncreaseMaxPopulation(int bonus)
        {
            maxPopulation += bonus;
            onPopulationChange?.Invoke();
        }

        public void IncreaseMaxFollowers(int bonus)
        {
            leader.maxFollowers += bonus;
        }

        public void AddWorker()
        {
            if (currentPopulation >= maxPopulation)
            {
                MessageManager.Message("Ooops", "Reached Max population. Upgrade population limit to keep growing", null, null);
                return;
            }

            CreateWorker();
        }

        public void IncreaseMaxHealth(float bonus)
        {
            workerStats.healthBonus += bonus;
            AntStats playerStats = (AntStats)playerAnt.GetComponent<StatsManager>().values;
            playerStats.healthBonus += bonus;

            foreach (GameObject ant in allAntsList)
            {
                ant.GetComponent<Health>().Heal(bonus);
            }
        }

        public void IncreaseMaxDamage(float bonus)
        {
            workerStats.damageBonus += bonus;
            AntStats stats = (AntStats)playerAnt.GetComponent<StatsManager>().values;
            stats.damageBonus += bonus;
        }

        public void IncreaseMaxSpeed(float bonus)
        {
            workerStats.speedBonus += bonus;
            AntStats stats = (AntStats)playerAnt.GetComponent<StatsManager>().values;
            stats.speedBonus += bonus;
        }

        public void IncreaseCarryCapacity(float bonus)
        {
            workerStats.carryCapacityBonus += bonus;
            AntStats stats = (AntStats)playerAnt.GetComponent<StatsManager>().values;
            stats.carryCapacityBonus += bonus;
        }

        #endregion

    }
}
