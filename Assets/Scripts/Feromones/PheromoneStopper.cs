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
            List<GameObject> CloseAnts = new List<GameObject>(detector.GetEntitiesInLayer(LayerManager.workerLayer));
            GameObject player = detector.GetEntityInLayer(LayerManager.playerLayer);
            if (player != null)
            {

            }

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