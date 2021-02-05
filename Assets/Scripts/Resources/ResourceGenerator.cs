using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Map;
using RPG.Core;
using RPG.Harvest;

namespace RPG.Resources
{
    public class ResourceGenerator : MonoBehaviour  //@todo acount for terrain height to avoid underground spawns
    {
        [SerializeField] GameObject foodPrefab, enemyPrefab;
        float spawnTimer;
        const float spawnTime = 60;

        [SerializeField] EnemyStats[] enemyStats;

        Vector3 nestPosition;

        [SerializeField] float baseTierDistance = 75;
        [SerializeField] float maxDistance = 450;
        [SerializeField] float startAngle, maxAngle;
        List<GameObject> activeResources;
        List<Vector3> things = new List<Vector3>();

        [SerializeField] Vector3 gizmoPos;
        [SerializeField] bool isenabled;
        [SerializeField] int tier;

        private void Start()
        {
            activeResources = new List<GameObject>();
            GameObject nest = GameObject.FindGameObjectWithTag("Nest");
            if (nest != null) nestPosition = nest.transform.position;

            SpawnFood(0, RandomPosition(0));
            SpawnFood(0, RandomPosition(0));
            SpawnFood(0, RandomPosition(0));

            SpawnFood(1, RandomPosition(1));
            SpawnFood(1, RandomPosition(1));
            SpawnEnemy(1, RandomPosition(1));

            SpawnFood(2, RandomPosition(2));
            SpawnEnemy(2, RandomPosition(2));
            SpawnEnemy(2, RandomPosition(2));

            SpawnEnemy(3, RandomPosition(3));
            SpawnEnemy(3, RandomPosition(3));
            SpawnEnemy(3, RandomPosition(3));

        }

        private void Update()
        {
            if (isenabled)
            {
                isenabled = false;
                gizmoPos = RandomPosition(tier);
            }
        }

        private void SpawnFood(int tier, Vector3 position)
        {
            Vector3 spawnPosition = position + nestPosition;
            spawnPosition.y = MapTools.getTerrainHeight(spawnPosition);
            GameObject.Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        }

        private void SpawnEnemy(int tier, Vector3 position)
        {
            EnemyStats stats = enemyStats[tier];

            Vector3 spawnPosition = position + nestPosition;
            spawnPosition.y = MapTools.getTerrainHeight(spawnPosition);
            GameObject newEnemy = GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            SkinnedMeshRenderer renderer=newEnemy.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.materials = stats.materials;
            GameObject model = renderer.gameObject; 
            model.transform.localScale *= stats.scale;
            newEnemy.GetComponent<StatsManager>().values= stats;
            newEnemy.GetComponent<HarvestTarget>().SetFoodAmount(stats.FoodAmount);
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