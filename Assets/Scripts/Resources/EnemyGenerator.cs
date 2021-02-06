using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Map;
using RPG.Core;
using RPG.Harvest;
using RPG.Control;

namespace RPG.Resources
{
    public class EnemyGenerator : MonoBehaviour  //@todo acount for terrain height to avoid underground spawns
    {
        [SerializeField] GameObject enemyPrefab;
        float spawnTimer;
        const float spawnTime = 60;

        [SerializeField] EnemyStats[] enemyStats;

        [SerializeField] PatrolPath[] editorDefinedPatrolsTierI;
        [SerializeField] PatrolPath[] editorDefinedPatrolsTierII;
        [SerializeField] PatrolPath[] editorDefinedPatrolsTierIII;

        Dictionary<PatrolPath, PatrolData> patrolInformation;
        float newPatrolCountDown;
        float timeBetweenNewPatrols = 60;
        [SerializeField] int[] startingEnemies;
        Vector3 nestPosition;

        [SerializeField] float baseTierDistance = 75;
        [SerializeField] float maxDistance = 450;
        [SerializeField] float startAngle, maxAngle;
        List<GameObject> activeResources;


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
            activeResources = new List<GameObject>();
            GameObject nest = GameObject.FindGameObjectWithTag("Nest");
            if (nest != null) nestPosition = nest.transform.position;

            createGuardian(3, transform.position);

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
            newPatrolCountDown -= Time.deltaTime;
            if (newPatrolCountDown < 0)
            {
                newPatrolCountDown = timeBetweenNewPatrols;
                PatrolPath emptyPatrol = GetEmptyPatrol();
                if (emptyPatrol != null)
                {
                    CreatePatrol(emptyPatrol);
                }
            }

            if (Input.GetKeyDown(KeyCode.A)) CreateAttackWave(0, 3);
            if (Input.GetKeyDown(KeyCode.S)) CreateAttackWave(1, 3);
            if (Input.GetKeyDown(KeyCode.D)) CreateAttackWave(2, 3);
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

            SkinnedMeshRenderer renderer = newEnemy.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.materials = stats.materials;

            GameObject model = renderer.gameObject;
            model.transform.parent.localScale *= stats.scale;   // if scale is applied to newEnemy hitboxes goes kaboom. imported gameObject is the parent of the model so we scale that 

            newEnemy.GetComponent<StatsManager>().values = stats;
            newEnemy.GetComponent<HarvestTarget>().SetFoodAmount(stats.FoodAmount);
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
    }
}