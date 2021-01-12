using System;
using UnityEngine;
namespace RPG.Core
{
    public class Health : MonoBehaviour
    {
        bool isDead;
        public bool IsDead()
        {
            return isDead;
        }

        [SerializeField] float maxHealth = 100f, health;

        private void Start()
        {
            health = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            health = Mathf.Clamp(health - amount, 0, maxHealth);
            if (health == 0)
            {
                Die();
            }
        }

        void Die()
        {
            if (isDead) return;

            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();

        }
    }
}