using UnityEngine;
using RPG.Movement;
using RPG.Core;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        [SerializeField] float weaponRange = 2.0f;
        [SerializeField] float timeBetweenAtacks = 12.0f;
        Health target;
        float timeSinceLastAttack = Mathf.Infinity;

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return;

            if (GetIsInRange())
            {
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
            else
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }

        }

        private void AttackBehaviour()
        {
            LookAt(target.transform);
            

            if (timeSinceLastAttack > timeBetweenAtacks)
            {
                if (target.GetComponent<Health>().IsDead)
                {
                    Cancel();
                    return;
                }

                GetComponent<Animator>().ResetTrigger("stopAttack");
                GetComponent<Animator>().SetTrigger("attack");
                Hit();// @ToDo: integrate Hit event in animation
                timeSinceLastAttack = 0;
            }
        }

        private void LookAt(Transform target)//Looks at transform only rotating in Y axys
        {
            Quaternion rotation = transform.rotation;
            transform.LookAt(target);

            rotation.y = transform.rotation.y;
            transform.rotation = rotation;
        }

        private bool GetIsInRange()
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.transform.position) < weaponRange;
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) return false;
            Health target = combatTarget.GetComponent<Health>();
            return target != null && !target.IsDead;
        }

        public void Attack(GameObject target)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            this.target = target.GetComponent<Health>();
        }

        public void Cancel()
        {
            target = null;
            GetComponent<Animator>().SetTrigger("stopAttack");
            GetComponent<Animator>().ResetTrigger("attack");
        }

        public void StartAction()
        {
            throw new System.NotImplementedException();
        }

        //Animator Event
        void Hit()
        {
            if (target == null) return;
            target.TakeDamage(5);
        }

    }

}