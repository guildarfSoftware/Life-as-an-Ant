using UnityEngine;
namespace RPG.Core
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "new BaseStats")]
    public class BaseStats : ScriptableObject
    {
        [SerializeField] float baseHealth;
        public float healthBonus;
        public float Health => baseHealth + healthBonus;

        [SerializeField] float baseDamage;
        public float damageBonus;
        public float Damage => baseDamage + damageBonus;

        [SerializeField] float baseSpeed;
        public float speedBonus;
        public float Speed => baseSpeed + speedBonus;

        public virtual void ResetBonus()
        {
            healthBonus = 0;
            damageBonus = 0;
            speedBonus = 0;
        }

    }
}