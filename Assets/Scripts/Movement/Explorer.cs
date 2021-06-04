using UnityEngine;
using RPG.Core;
using System;
using UnityEngine.AI;
using RPG.Map;

namespace RPG.Movement
{
    public class Explorer : MonoBehaviour, IAction
    {

        float range = 2f;
        public bool ExplorationDone { get => wanderTimer <= 0; }
        float wanderTimer = 0;
        [SerializeField] const float ExploreTime = 10;

        [SerializeField] float maxTravel = 12, minTravel = 6;
        [SerializeField] float maxAngle = 315, minAngle = 45;

        Vector3 targetPosition;
        public bool wandering { private set; get; }

        private void Update()
        {
            wanderTimer -= Time.deltaTime;
            if (ExplorationDone)
            {
                wandering = false;
                Cancel();
            }
            else
            {
                WanderBehaviour();
            }

        }

        private void WanderBehaviour()
        {
            if (targetPosition != default(Vector3))
            {
                if (Vector3.Distance(transform.position, targetPosition) < range)
                {
                    int attemps = 0;
                    do
                    {
                        targetPosition = GetRandomPosition(transform.position);
                        attemps++;
                    } while (!MapTools.SampleTerrainPosition(targetPosition, out targetPosition) && attemps < 10);

                    if (attemps >= 10)
                    {
                        targetPosition = transform.position;
                        transform.Rotate(new Vector3(0, 90, 0));
                    }

                }
                else
                {
                    GetComponent<Mover>().MoveTo(targetPosition);
                }
            }
        }

        public void Wander(float wanderTime = -1)
        {
            if (!GetComponent<ActionScheduler>().StartAction(this)) return;
            targetPosition = GetRandomPosition(transform.position);
            wandering = true;
            this.wanderTimer = wanderTime == -1 ? float.MaxValue : wanderTime;
        }

        private Vector3 GetRandomPosition(Vector3 position)
        {
            float module = UnityEngine.Random.Range(minTravel, maxTravel);
            Vector3 localPos = -Vector3.forward * module;        //points backwards to ease rotation 
            float rotation = UnityEngine.Random.Range(minAngle, maxAngle);
            localPos = Quaternion.Euler(0, rotation, 0) * localPos;

            Vector3 worldPos = transform.rotation * localPos + transform.position;
            worldPos.y = MapTools.getTerrainHeight(worldPos);
            return worldPos;
        }

        public void Cancel()
        {
            targetPosition = default(Vector3);
            wandering = false;
            wanderTimer = 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(targetPosition, 1f);
        }

        public bool isCancelable()
        {
            return true;
        }
    }
}