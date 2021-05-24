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

        public static EntityDetector CreateDetector(GameObject parent)
        {
            GameObject detector = new GameObject("Detector");
            detector.transform.position = parent.transform.position;
            detector.transform.parent = parent.transform;

            return detector.AddComponent<EntityDetector>();
        }

        private void Start()
        {
            SphereCollider spCollider = GetComponent<SphereCollider>();
            spCollider.isTrigger = true;
            spCollider.radius = scanRange;
        }

        private void Update()
        {
            for (int i = closeEntities.Count - 1; i >= 0; i--)    // @TODO change this method and find other way to remove null members
            {
                if (closeEntities[i] == null) closeEntities.RemoveAt(i);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            closeEntities.Add(other.gameObject);
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
            List<GameObject> rList = new List<GameObject>();
            for (int i = 0; i < closeEntities.Count; i++)
            {
                GameObject entity = closeEntities[i];
                if (entity != null && entity.tag == tag) rList.Add(entity);
            }

            return rList;
        }
        public GameObject GetEntityWithTag(string tag)
        {
            List<GameObject> rList = new List<GameObject>();
            for (int i = 0; i < closeEntities.Count; i++)
            {
                GameObject entity = closeEntities[i];
                if (entity != null && entity.tag == tag) return entity;
            }
            return null;
        }

        public void Sort(Comparison<GameObject> comparison)
        {
            closeEntities.Sort(comparison);
        }

        internal void Reset()
        {
            closeEntities.Clear();
        }
    }
}