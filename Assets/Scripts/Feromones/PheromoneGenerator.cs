using UnityEngine;
using RPG.Harvest;
using RPG.Combat;
using System.Collections;

namespace RPG.Pheromones
{
    public class PheromoneGenerator : MonoBehaviour
    {
        //[SerializeField] GameObject pheromonePrefab;
        bool safeExit;
        float timeBetweenWaypoints = 1;
        [SerializeField] bool generating;
        PheromoneType generatingType;

        Harvester harvester;
        Fighter fighter;
        PheromoneWaypoint lastGenerated = null;
        float timeOutOfCOmbat;
        const float combatCooldown = 15;

        private void Start()
        {
            StartCoroutine(GenerationProcess());

            harvester = GetComponent<Harvester>();
            harvester.fooodGrabbed += () => {StartGeneration(PheromoneType.Harvest); };
            harvester.foodDeposit += () => { generating = false; };

            fighter = GetComponent<Fighter>();
            fighter.EnterCombat += () => { StartGeneration(PheromoneType.Combat); };
        }

        private void Update()
        {
            timeOutOfCOmbat += Time.deltaTime;
            if(generatingType == PheromoneType.Combat && timeOutOfCOmbat > combatCooldown)
            {
                generating = false;
            }
        }

        IEnumerator GenerationProcess()
        {
            while (!safeExit)
            {
                if (generating)
                {
                    GameObject waypointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    waypointObject.transform.position = transform.position;

                    PheromoneWaypoint waypointScript = waypointObject.AddComponent<PheromoneWaypoint>();
                    waypointScript.pheromoneType = generatingType;

                    if (lastGenerated != null)
                    {
                        waypointScript.distanceFromSource = lastGenerated.distanceFromSource+1;
                        lastGenerated.nextWaypoint = waypointScript;
                    }
                    waypointScript.previousWaypoint = lastGenerated;
                    lastGenerated = waypointScript;
                }

                yield return new WaitForSeconds(timeBetweenWaypoints);
            }
        }

        void StartGeneration(PheromoneType type)
        {
            generating = true;

            if (type == PheromoneType.Combat)
            {
                timeOutOfCOmbat = 0;
            }

            if (type == generatingType) return;
            lastGenerated = null;
            generatingType = type;
        }

    }
}