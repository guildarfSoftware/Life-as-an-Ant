using UnityEngine;

namespace RPG.Harvest
{    
    public class HarvestTarget : MonoBehaviour
    {
        [SerializeField] float remainingResource = 100;
        public bool IsEmpty { get => remainingResource == 0; }

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
        
    }
}