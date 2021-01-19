using UnityEngine;

namespace RPG.Harvest
{
    public class Storage : MonoBehaviour
    {
        [SerializeField] float maxCapacity=200;
        [SerializeField] float storedAmount;

        public bool IsFull { get => storedAmount >= maxCapacity; }

        public void StoreResource(float amount)
        {
            storedAmount += amount;
            storedAmount = Mathf.Min(storedAmount, maxCapacity);
        }

    }
}