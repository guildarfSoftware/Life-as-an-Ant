using UnityEngine;
using RPG.Core;
using System.Collections.Generic;

namespace RPG.Pheromones
{
    public class PheromoneStopper : MonoBehaviour
    {
        EntityDetector detector;

        float stopPheromoneCounter;
        float stopPheromoneTime = 0.2f;

        private void Start()
        {
            detector = GetComponentInChildren<EntityDetector>();
            if (detector == null) detector = EntityDetector.CreateDetector(gameObject);
        }

        private void Update()
        {
            stopPheromoneCounter += Time.deltaTime;
            if (stopPheromoneCounter > stopPheromoneTime)
            {
                stopPheromoneCounter = 0;
                StopPheromoneGenerator();
            }
        }

        void StopPheromoneGenerator()
        {
            GameObject player = detector.GetEntityInLayer(LayerManager.playerLayer);
            if (player != null)
            {
                PheromoneGenerator generator = player.GetComponent<PheromoneGenerator>();
                if (generator != null)
                {
                    generator.StopGeneration(transform);
                }
            }

            var CloseAnts = detector.GetEntitiesInLayer(LayerManager.workerLayer);
            foreach (GameObject ant in CloseAnts)
            {
                PheromoneGenerator generator = ant.GetComponent<PheromoneGenerator>();
                if (generator != null)
                {
                    generator.StopGeneration(transform);
                }
            }
        }
    }

}