using UnityEngine;
using RPG.Core;
using System.Collections.Generic;

namespace RPG.Pheromones
{
    [RequireComponent(typeof(EntityDetector))]
    public class PheromoneStopper : MonoBehaviour
    {
        EntityDetector detector;

        private void Start()
        {
            detector = GetComponent<EntityDetector>();
        }

        private void Update()
        {
            StopPheromoneGenerator();
        }

        void StopPheromoneGenerator()
        {
            List<GameObject> CloseAnts = detector.GetEntitiesWithTag("Worker");
            GameObject player = detector.GetEntityWithTag("Player");
            if (player != null) CloseAnts.Add(player);

            foreach (GameObject ant in CloseAnts)
            {
                PheromoneGenerator generator = ant.GetComponent<PheromoneGenerator>();
                if (generator != null)
                {
                    generator.StopGeneration();
                }
            }
        }
    }

}