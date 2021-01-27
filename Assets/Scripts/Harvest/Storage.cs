using System;
using UnityEngine;

namespace RPG.Harvest
{
    public class Storage : MonoBehaviour
    {
        [SerializeField] int maxCapacity = 200;

        public int MaxCapacity { get => maxCapacity; }
        public int storedAmount { get; private set; }
        public Action onStoreChange;

        public bool IsFull { get => storedAmount >= maxCapacity; }

        public void StoreResource(int amount)
        {
            storedAmount += amount;
            storedAmount = Mathf.Min(storedAmount, maxCapacity);
            onStoreChange?.Invoke();
        }

        internal void Consume(int amount)
        {
            storedAmount-=amount;
            storedAmount = Mathf.Max(storedAmount,0);
            onStoreChange?.Invoke();
        }
    }
}