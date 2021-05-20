using UnityEngine;
using System.Collections;
using RPG.Core;
using System;
using System.Collections.Generic;

namespace RPG.Pheromones
{
    public class PheromoneGenerator : MonoBehaviour
    {
        bool safeExit;
        float timeBetweenWaypoints = 0.5f;
        [SerializeField] bool generating;
        PheromoneType generatingType;
        PheromoneWaypoint lastGenerated = null;
        float pheromoneDuration;
        const float combatCooldown = 15f;

        [SerializeField] Gradient gradientHarvest, gradientCombat;
        [SerializeField] Material trailMaterial;

        private void Start()
        {
            StartCoroutine(GenerationProcess());
        }

        private void Update()
        {
            pheromoneDuration -= Time.deltaTime;
            if (pheromoneDuration < 0)
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
                    AddPheromoneWaypoint(transform.position);

                    yield return new WaitForSeconds(timeBetweenWaypoints);
                }
                yield return null;
            }
        }

        private PheromoneWaypoint CreatePheromoneWaypoint(Vector3 position)
        {
            string name = (generatingType == PheromoneType.Combat ? "Combat" : "Harvest") + " Waypoint";
            name += " " + (lastGenerated == null ? 0 : lastGenerated.distanceFromSource + 1);

            GameObject waypointObject = new GameObject(name);//GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypointObject.transform.position = position;

            (waypointObject.AddComponent<SphereCollider>()).isTrigger = true;
            waypointObject.tag = generatingType == PheromoneType.Combat ? "PheromoneCombat" : "PheromoneHarvest";

            PheromoneWaypoint waypointScript = waypointObject.AddComponent<PheromoneWaypoint>();
            waypointScript.SetPheromoneType(generatingType);

            return waypointScript;
        }

        public void StartGeneration(PheromoneType type, float duration = -1)
        {
            if (type == PheromoneType.None) return;
            if (duration != -1)
            {
                pheromoneDuration = duration;
            }
            else
            {
                pheromoneDuration = float.MaxValue;
            }

            if (type != generatingType)
            {
                StopGeneration(); //Resets trail
                generatingType = type;
            }

            generating = true;
        }

        public void StopGeneration(Transform endPosition = null)
        {
            if(endPosition == null) endPosition=transform;
            if (generating) AddPheromoneWaypoint(endPosition.position);
            DetachTrailGenerator();
            generating = false;
            lastGenerated = null;
        }


        private void AddPheromoneWaypoint(Vector3 position)
        {
            PheromoneWaypoint newWaypoint = CreatePheromoneWaypoint(position);

            if (lastGenerated != null)
            {
                newWaypoint.distanceFromSource = lastGenerated.distanceFromSource + 1;
                lastGenerated.nextWaypoint = newWaypoint;
            }
            else //first waypoint of trail
            {
                AttachTrailGenerator();
            }

            newWaypoint.previousWaypoint = lastGenerated;
            lastGenerated = newWaypoint;
        }

        private void AttachTrailGenerator()
        {
            GameObject trailGenerator = new GameObject("Trail Generator");

            TrailRenderer trailRenderer = trailGenerator.AddComponent<TrailRenderer>();
            trailRenderer.time = float.MaxValue;
            trailRenderer.colorGradient = (generatingType == PheromoneType.Combat) ? gradientCombat : gradientHarvest;
            trailRenderer.material = trailMaterial;

            trailGenerator.transform.position = this.transform.position;
            trailGenerator.transform.parent = this.transform;

            // FollowTransform follow = trailGenerator.AddComponent<FollowTransform>();
            // follow.target = this.transform;
        }

        void DetachTrailGenerator()//puts the trail follower in the last waypoint generator so they are destroyed together
        {
            TrailRenderer trailGenerator = GetComponentInChildren<TrailRenderer>();

            if (trailGenerator != null)
            {
                trailGenerator.emitting = false;
                if (lastGenerated != null)
                {
                    trailGenerator.transform.parent = lastGenerated.transform;
                }
                else
                {
                    trailGenerator.time = 10;
                    Destroy(trailGenerator.gameObject, 10);
                }
            }
        }



    }
}