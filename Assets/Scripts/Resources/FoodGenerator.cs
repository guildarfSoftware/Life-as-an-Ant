using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Map;
using RPG.Core;
using RPG.Harvest;
using RPG.Sounds;

namespace RPG.Resources
{

    public class FoodGenerator : MonoBehaviour
    {
        [SerializeField] GameObject foodPrefab;
        float spawnTimer;
        [SerializeField] bool spawnAtStart;
        [SerializeField] float maxSpawnDistance, minSpawnDistance;
        [SerializeField] float spawnTime = 90;
        List<GameObject> activeResources;
        // Start is called before the first frame update
        void Start()
        {
            activeResources = new List<GameObject>();
            if(spawnAtStart)SpawnFood(RandomPosition());
        }

        // Update is called once per frame
        void Update()
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnTime)
            {
                spawnTimer = 0;
                SpawnFood(RandomPosition());
            }
        }

        private void SpawnFood(Vector3 position)
        {
            Vector3 spawnPosition;
            MapTools.SampleTerrainPosition(position, out spawnPosition);
            GameObject food = GameObject.Instantiate(foodPrefab);
            food.transform.localPosition = spawnPosition;
            food.transform.SetParent(transform);
            SoundEffects.PlaySound(ClipId.foodSpawn);
        }

        private Vector3 RandomPosition()
        {
            Vector3 randomDirection = UnityEngine.Random.onUnitSphere;

            Vector3 localPosition = randomDirection * UnityEngine.Random.Range(maxSpawnDistance, minSpawnDistance);

            Vector3 position = localPosition + transform.position;
            position.y = MapTools.getTerrainHeight(position);

            if(!MapTools.SampleTerrainPosition(position, out position))
            {
                return RandomPosition();
            } 

            return position;

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, minSpawnDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxSpawnDistance);

        }
    }
}
