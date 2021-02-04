using System;
using UnityEngine;

namespace RPG.Harvest
{
    public class Storage : MonoBehaviour
    {
        [SerializeField] float maxCapacity = 200;

        public float MaxCapacity { get => maxCapacity; }
        public float storedAmount { get; private set; }
        public Action onStoreChange;

        public bool IsFull { get => storedAmount >= maxCapacity; }

        public void StoreResource(float amount)
        {
            storedAmount += amount;
            storedAmount = Mathf.Min(storedAmount, maxCapacity);
            onStoreChange?.Invoke();
        }

        internal void IncreaseMaxCapacity(float storageBonus)
        {
            maxCapacity += storageBonus;
        }

        internal void Consume(float amount)
        {
            storedAmount-=amount;
            storedAmount = Mathf.Max(storedAmount,0);
            onStoreChange?.Invoke();
        }
    }
}