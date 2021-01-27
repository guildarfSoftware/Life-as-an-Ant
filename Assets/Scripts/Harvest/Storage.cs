using UnityEngine;

namespace RPG.Harvest
{
    public class Storage : MonoBehaviour
    {
        [SerializeField] int maxCapacity = 200;

        public int MaxCapacity { get => maxCapacity; }
        public int storedAmount { get; private set; }

        public bool IsFull { get => storedAmount >= maxCapacity; }

        public void StoreResource(int amount)
        {
            storedAmount += amount;
            storedAmount = Mathf.Min(storedAmount, maxCapacity);
        }

    }
}