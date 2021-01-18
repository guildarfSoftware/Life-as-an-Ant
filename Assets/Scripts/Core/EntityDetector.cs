using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    [RequireComponent(typeof(SphereCollider))]
    public class EntityDetector : MonoBehaviour
    {
        float scanRange = 10f;

        List<GameObject> closeEntities = new List<GameObject>();

        public List<GameObject> CloseEntities { get => closeEntities; }

        private void Start()
        {
            SphereCollider spCollider = GetComponent<SphereCollider>();
            spCollider.radius = scanRange;
        }

        private void Update()
        {
            for (int i = closeEntities.Count-1; i >= 0; i--)    // @TODO change this method and find other way to remove null members
            {
                if(closeEntities[i]==null) closeEntities.RemoveAt(i);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case "Enemy":
                case "Food":
                case "PheromoneCombat":
                case "PheromoneHarvest":
                    {
                        closeEntities.Add(other.gameObject);
                        print("Added " + other.gameObject.name);
                        break;
                    }
                default: return;
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (closeEntities.Contains(other.gameObject))
            {
                closeEntities.Remove(other.gameObject);
                print("Removed " + other.gameObject.name);
            }
        }
    }
}