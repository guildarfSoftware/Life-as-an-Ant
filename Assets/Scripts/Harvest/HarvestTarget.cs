using RPG.Core;
using UnityEngine;

namespace RPG.Harvest
{    
    public class HarvestTarget : MonoBehaviour
    {
        [SerializeField] int remainingResource = 100;
        public bool IsEmpty { get => remainingResource == 0; }

        private void Start()
        {
            if(GetComponent<Health>()!=null) GetComponent<Health>().OnDeath += ()=>gameObject.tag = "Food"; //if it was alive, on death change tag to food 
        }


        public int GrabResource(int triedAmount)
        {
            int grabedAmount = 0;
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
        
    }
}