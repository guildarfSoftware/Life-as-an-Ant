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

        float maxHealth { get => stats.values.Health; }

        public float currentHealth { private set; get; }
        public float MaxHealth { get => maxHealth; }
        public Action onDamaged { get;  set; }

        private void Awake()
        {
            stats = GetComponent<StatsManager>();
            currentHealth = maxHealth;
            OnHealthChange?.Invoke();
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            OnHealthChange?.Invoke();
            onDamaged?.Invoke();
            if (currentHealth == 0)
            {
                Die();
            }
        }

        void Die()
        {
            if (isDead) return;

            isDead = true;

            GetComponent<Animator>().SetTrigger("die");
            GetComponent<Animator>().SetBool("Dead", true);
            GetComponent<ActionScheduler>().CancelCurrentAction();

            OnDeath?.Invoke();
        }

        internal void Heal(float amount)
        {
            if (currentHealth == maxHealth) return;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChange?.Invoke();
        }

        internal void Revive()
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChange?.Invoke();
            GetComponent<Animator>().ResetTrigger("die");
            GetComponent<Animator>().SetBool("Dead", false);
        }
    }
}