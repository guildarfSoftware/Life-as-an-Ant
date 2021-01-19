using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Movement
{
    public class Explorer : MonoBehaviour, IAction
    {

        float epsilon = 0.1f;
        public bool TimeOut { get => wanderTimer <= 0; }
        float wanderTimer = 0;
        float coolDownTimer = 0;
        const float RestTime = 30;
        public bool onCooldown {get=> coolDownTimer > 0;}

        Vector3 targetPosition;

        private void Update()
        {
            wanderTimer -= Time.deltaTime;
            coolDownTimer -= Time.deltaTime;
            if (TimeOut)
            {
                Cancel();
            }

            WanderBehaviour();

        }

        private void WanderBehaviour()
        {
            if(targetPosition != default(Vector3))
            {
                if (Vector3.Distance(transform.position, targetPosition)  < epsilon)
                {
                    targetPosition = GetRandomPosition(transform.position);
                    GetComponent<Mover>().MoveTo(targetPosition);
                }
                else
                {
                    GetComponent<Mover>().MoveTo(targetPosition);
                }
            }
        }

        public void Wander(float wanderTime)
        {
            if(onCooldown) return;
            targetPosition = GetRandomPosition(transform.position);
            this.wanderTimer = wanderTime;
            coolDownTimer = RestTime + wanderTime;
        }

        private Vector3 GetRandomPosition(Vector3 position)
        {
            float module = UnityEngine.Random.Range(6f, 12f);
            Vector3 localPos = -Vector3.forward * module;        //points backwards to ease rotation 
            float rotation = UnityEngine.Random.Range(45f, 315f);
            localPos = Quaternion.Euler(0, rotation, 0) * localPos;

            Vector3 worldPos = transform.rotation*localPos +transform.position;

            return worldPos;
        }

        public void Cancel()
        {
            targetPosition=default(Vector3);
        }

    }
}