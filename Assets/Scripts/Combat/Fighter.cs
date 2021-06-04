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
        bool onAnimation;
        StatsManager stats;
        Health health;
        [SerializeField] float damage { get => stats.values.Damage; }
        Health target;
        public event Action InCombat;

        float healingSpeed = 0.5f;

        float inCombatCounter;
        float inCombatTime = 5f;
        float timeSinceLastAttack = Mathf.Infinity;

        private void Awake()
        {
            health = GetComponent<Health>();
        }
        void OnEnable()
        {
            health.onDamaged += EnterCombat;
            stats = GetComponent<StatsManager>();
        }

        void OnDisable()
        {
            health.onDamaged -= EnterCombat;
        }

        private void Update()
        {
            if(health.IsDead) return;
            timeSinceLastAttack += Time.deltaTime;
            inCombatCounter -= Time.deltaTime;

            if (inCombatCounter < 0)
            {
                health.Heal(healingSpeed * Time.deltaTime);
            }

            if (target == null)
            {
                return;
            }

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
                onAnimation = true;
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
            if (!GetComponent<ActionScheduler>().StartAction(this)) return;
            this.target = target.GetComponent<Health>();
        }

        public void Cancel()
        {
            target = null;
            GetComponent<Animator>().SetTrigger("stopAttack");
            GetComponent<Animator>().ResetTrigger("attack");
            onAnimation = false;
        }

        //Animator Event
        void Hit()
        {
            if (target == null) return;
            target.TakeDamage(damage);
            EnterCombat();
            onAnimation = false;

        }

        public bool isCancelable()
        {
            return !onAnimation;
        }
    
        void EnterCombat()
        {
            inCombatCounter = inCombatTime;
            InCombat?.Invoke();
        }
    }

}