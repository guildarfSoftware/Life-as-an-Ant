using System;
using RPG.Core;
using UnityEngine;

namespace RPG.Harvest
{    
    public class HarvestTarget : MonoBehaviour
    {
        [SerializeField] float remainingResource = 100;
        public bool IsEmpty { get => remainingResource == 0; }

        private void Start()
        {
            Health health =GetComponent<Health>();
            if(health!=null) health.OnDeath += (gObject)=>gObject.layer = LayerManager.foodLayer; //if it was alive, on death change tag to food 
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

            if(IsEmpty)
            {
                Destroy(gameObject);
            }

            return grabedAmount;
        }

        internal void SetFoodAmount(float foodAmount)
        {
            remainingResource = foodAmount;
        }
    }
}