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
        public Action OnHealthChange;
        public bool IsDead
        {
            get => isDead;
        }

        StatsManager stats;

        float maxHealth {get=> stats.values.Health;}

        public float currentHealth { private set; get; }
        public float MaxHealth { get => maxHealth; }

        private void Start()
        {
            stats = GetComponent<StatsManager>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            OnHealthChange?.Invoke();
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

        internal void Heal(float v)
        {
            currentHealth += v;
            currentHealth = Mathf.Min(currentHealth,maxHealth);
            OnHealthChange?.Invoke();
        }
    }
}