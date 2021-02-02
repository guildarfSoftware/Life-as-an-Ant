using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using System;

namespace RPG.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Health))]
    public class Mover : MonoBehaviour, IAction
    {
        NavMeshAgent navMeshAgent;
        Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.Warp(transform.position);    
        }

        // Update is called once per frame
        void Update()
        {
            if (IsDead())
            {
                Cancel();
                navMeshAgent.enabled = false;
            }
            UpdateAnimator();
        }

        private bool IsDead()
        {
            return GetComponent<Health>().IsDead;
        }

        void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            animator.SetFloat("forwardSpeed", speed);
        }

        public void StartMovement(Vector3 destination)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination);
        }

        public void MoveTo(Vector3 destination)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            if(!navMeshAgent.enabled) return;
            navMeshAgent.isStopped = true;
        }
    }
}