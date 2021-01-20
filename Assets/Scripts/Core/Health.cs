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

        [SerializeField] float maxHealth = 100f, health;

        private void Start()
        {
            health = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            health = Mathf.Clamp(health - amount, 0, maxHealth);
            OnDamaged?.Invoke();
            if (health == 0)
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