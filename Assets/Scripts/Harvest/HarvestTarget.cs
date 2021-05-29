using System;
using RPG.Core;
using UnityEngine;

namespace RPG.Harvest
{
    public class HarvestTarget : MonoBehaviour
    {
        float remainingResource = 100;
        [SerializeField] float maxResource = 100;
        public bool IsEmpty { get => remainingResource == 0; }
        public float RemainingFood { get => remainingResource; }
        public float MaxFood { get => maxResource; }


        private void Start()
        {
            remainingResource = maxResource;
        }


        public float GrabResource(float triedAmount)
        {
            float grabedAmount = 0;
            if (triedAmount > remainingResource)
            {
                grabedAmount = remainingResource;
            }
            else
            {
                grabedAmount = triedAmount;
            }
            remainingResource -= grabedAmount;

            if (IsEmpty)
            {
                Destroy(gameObject);
            }

            return grabedAmount;
        }

        internal void SetFoodAmount(float foodAmount)
        {
            maxResource = foodAmount;
            remainingResource = maxResource;
        }
    }
}