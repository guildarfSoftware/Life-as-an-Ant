using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    [RequireComponent(typeof(SphereCollider))]
    public class EntityDetector : MonoBehaviour
    {
        [SerializeField] float scanRange = 10f;

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
            }
        }

        public List<GameObject> GetEntitiesWithTag(string tag)
        {
            List<GameObject> rList =  new List<GameObject>();
            for (int i = 0; i < closeEntities.Count; i++)
            {
                GameObject entity = closeEntities[i];
                if (entity != null && entity.tag == tag) rList.Add(entity);
            }

            return rList;
        } 

        public void Sort(Comparison<GameObject> comparison)
        {
            closeEntities.Sort(comparison);
        }

    }
}