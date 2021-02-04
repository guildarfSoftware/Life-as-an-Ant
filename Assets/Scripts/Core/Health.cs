using System;
using UnityEngine;

namespace RPG.Core
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ActionScheduler))]
    public class Health : MonoBehaviour
    {
        [SerializeField] bool isDead;

        public Action OnDeath;
        public Action OnDamaged;
        public bool IsDead
        {
            get => isDead;
        }

        float maxHealth {get=> AntStats.Health;}

        public float currentHealth { private set; get; }
        public float MaxHealth { get => maxHealth; }

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            OnDamaged?.Invoke();
            if (currentHealth == 0)
            {
                Die();
            }
        }

        void Die()
        {
            if (isDead) return;

            isDead = true;

            OnDeath?.Invoke();

            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();

        }
    }
}