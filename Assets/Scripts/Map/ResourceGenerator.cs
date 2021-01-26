namespace RPG.Map
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ResourceGenerator : MonoBehaviour
    {
        [SerializeField] GameObject foodPrefab, enemyPrefab;
        float spawnTimer;
        const float spawnTime = 60;

        Vector3 nestPosition;

        [SerializeField] float baseTierDistance = 75;
        [SerializeField] float maxDistance = 450;
        [SerializeField] float startAngle, maxAngle;
        List<GameObject> activeResources;

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

        }

        private void SpawnFood(int tier, Vector3 position)
        {
            Vector3 spawnPosition = position + nestPosition;
            GameObject.Instantiate( foodPrefab , spawnPosition, Quaternion.identity);
        }

        private void SpawnEnemy(int tier, Vector3 position)
        {
            Vector3 spawnPosition = position + nestPosition;
            GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }

        private Vector3 RandomPosition(int tier)
        {
            float minDistance = baseTierDistance * tier;
            float maxDistance = baseTierDistance *(tier+1);

            float randomDistance = UnityEngine.Random.Range(minDistance,maxDistance);

            randomDistance =  Mathf.Min(randomDistance,maxDistance);

            float angle = UnityEngine.Random.Range(startAngle, maxAngle);

            Vector3 position = new Vector3(randomDistance,0,0);

            position = Quaternion.Euler(0, angle, 0) * position;

            return position;

        }
    }
}