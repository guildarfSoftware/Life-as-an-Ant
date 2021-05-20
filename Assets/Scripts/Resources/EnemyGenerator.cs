using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Map;
using RPG.Core;
using RPG.Harvest;
using RPG.Control;
using RPG.UI;

namespace RPG.Resources
{
    public class EnemyGenerator : MonoBehaviour  //@todo acount for terrain height to avoid underground spawns
    {
        public static EnemyGenerator instance;
        [SerializeField] GameObject enemyPrefab;
        float eventTimer;
        const float timeBetweenEvents = 90;
        private const int QueenTier = 3;
        [SerializeField] EnemyStats[] enemyStats;
        [SerializeField] PatrolPath[] editorDefinedPatrolsTierI;
        [SerializeField] PatrolPath[] editorDefinedPatrolsTierII;
        [SerializeField] PatrolPath[] editorDefinedPatrolsTierIII;

        Dictionary<PatrolPath, PatrolData> patrolInformation;
        float patrolTimer;
        float timeBetweenNewPatrols = 30;
        [SerializeField] int[] startingEnemies;
        Vector3 nestPosition;

        [SerializeField] float baseTierDistance = 75;
        [SerializeField] float maxDistance = 450;
        [SerializeField] float startAngle, maxAngle;
        List<GameObject> activeResources;

        [SerializeField]int dificultyValue; //increases on enemy kills and princess population
        private bool spawnerActive = true;

        struct PatrolData
        {
            public bool isEmpty;
            public int tier;

            public PatrolData(int tier, bool isEmpty)
            {
                this.isEmpty = isEmpty;
                this.tier = tier;
            }
        }

        private void Start()
        {
            if(instance != null) Debug.LogWarning("multiple instance of enemy generator actives");
            instance = this;
            activeResources = new List<GameObject>();
            GameObject nest = GameObject.FindGameObjectWithTag("Nest");
            if (nest != null) nestPosition = nest.transform.position;

            createGuardian(QueenTier, transform.position);

            for (int tier = 0; tier < startingEnemies.Length; tier++)
            {
                int enemyCount = startingEnemies[tier];
                for (int i = 0; i < enemyCount; i++)
                {
                    createGuardian(tier, RandomPosition(tier));
                }
            }

            patrolInformation = new Dictionary<PatrolPath, PatrolData>();

            foreach (PatrolPath patrol in editorDefinedPatrolsTierI)
            {
                patrolInformation.Add(patrol, new PatrolData(0, true));
            }
            foreach (PatrolPath patrol in editorDefinedPatrolsTierII)
            {
                patrolInformation.Add(patrol, new PatrolData(1, true));
            }
            foreach (PatrolPath patrol in editorDefinedPatrolsTierIII)
            {
                patrolInformation.Add(patrol, new PatrolData(2, true));
            }
        }

        private void Update()
        {
            if(!spawnerActive) return;
            eventTimer -= Time.deltaTime;
            patrolTimer -= Time.deltaTime;
            if (patrolTimer < 0)
            {
                patrolTimer = timeBetweenNewPatrols;
                PatrolPath emptyPatrol = GetEmptyPatrol();
                if (emptyPatrol != null)
                {
                    CreatePatrol(emptyPatrol);
                }
            }

            if (eventTimer < 0)
            {
                eventTimer = timeBetweenEvents;
                RandomEvent();
                IncreaseDifficulty((int)(timeBetweenEvents)/30); //increase dificulty 2 times per minute passed
            }
        }

        static public void IncreaseDifficulty(int amount)
        {
            instance.dificultyValue += amount;
        }

        void RandomEvent()
        {
            int randomValue = UnityEngine.Random.Range(0, dificultyValue);

            if (randomValue > 200)
            {
                CreateAttackWave(2, (randomValue-100) / 100);
            }
            else if (randomValue > 140)
            {
                CreateAttackWave(1, randomValue / 40);
            }
            else if (randomValue > 100)
            {
                CreateAttackWave(0, randomValue / 20);
            }
            else if (randomValue > 75)
            {
                createGuardian(2, RandomPosition(2));
            }
            else if (randomValue > 70)
            {
                CreateAttackWave(1, 4);
            }
            else if (randomValue > 50)
            {
                CreateAttackWave(1, 2);
            }
            else if (randomValue > 35)
            {
                CreateAttackWave(0, 4);
            }
            else if (randomValue > 25)
            {
                CreateAttackWave(1, 1);
            }
            else if (randomValue > 16)
            {
                createGuardian(2,RandomPosition(2));
            }
            else if (randomValue > 15)
            {
                CreateAttackWave(0, 2);
            }
            else if (randomValue > 10)
            {
                createGuardian(1, RandomPosition(1));
            }
            else if (randomValue > 5)
            {
                createGuardian(0, RandomPosition(0));
            }
        }

        private PatrolPath GetEmptyPatrol()
        {
            foreach (PatrolPath patrol in patrolInformation.Keys)
            {
                bool patrolEmpty = patrolInformation[patrol].isEmpty;
                if (patrolEmpty) return patrol;
            }
            return null;
        }

        void createGuardian(int tier, Vector3 position)
        {
            GameObject newEnemy = CreateMindlessEnemy(tier);

            if(tier == QueenTier)
            {
                newEnemy.GetComponent<Health>().OnDeath+= OnQueenDeath;
            }

            newEnemy.GetComponent<AIController>().SetGuardPosition(position);

        }

        void CreatePatrol(PatrolPath patrol)
        {
            PatrolData patrolData = patrolInformation[patrol];

            GameObject newEnemy = CreateMindlessEnemy(patrolData.tier);

            newEnemy.GetComponent<AIController>().SetPatrolPath(patrol);
            patrolData.isEmpty = false;
            patrolInformation[patrol] = patrolData;

        }

        void CreateAttackWave(int tier, int size)
        {
            for (int i = 0; i < size; i++)
            {
                createGuardian(tier, nestPosition);
            }
        }


        private GameObject CreateMindlessEnemy(int tier)
        {
            EnemyStats stats = enemyStats[tier];

            Vector3 spawnPosition = transform.position;
            spawnPosition.y = MapTools.getTerrainHeight(spawnPosition);

            MapTools.SampleTerrainPosition(spawnPosition, out spawnPosition);

            GameObject newEnemy = GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            newEnemy.transform.SetParent(transform);

            SkinnedMeshRenderer renderer = newEnemy.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.materials = stats.materials;

            GameObject model = renderer.gameObject;
            model.transform.parent.localScale *= stats.scale;   // if scale is applied to newEnemy hitboxes goes kaboom. imported gameObject is the parent of the model so we scale that 

            newEnemy.GetComponent<StatsManager>().values = stats;
            newEnemy.GetComponent<HarvestTarget>().SetFoodAmount(stats.FoodAmount);

            newEnemy.GetComponent<Health>().OnDeath += ()=>{IncreaseDifficulty(tier+1);};   //each enemy killed increases dificulty value

            return newEnemy;
        }

        private Vector3 RandomPosition(int tier)
        {
            float minDistance = baseTierDistance * tier;
            float maxDistance = baseTierDistance * (tier + 1);

            float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

            randomDistance = Mathf.Min(randomDistance, maxDistance);

            float angle = UnityEngine.Random.Range(startAngle, maxAngle);

            Vector3 position = new Vector3(randomDistance, 0, 0);

            position = Quaternion.Euler(0, angle, 0) * position;

            return position;

        }

        private void OnQueenDeath()
        {
            spawnerActive = false;
            MessageManager.Message("Congratulations",
                                    "You have finally defeated the queen, now the colony will be ok. Do you want to exit now?", 
                                    UIManager.LoadMenuScene,
                                    MessageManager.CloseMessage);
        }


    }
}