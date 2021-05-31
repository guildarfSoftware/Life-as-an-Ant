using System;
using System.Collections.Generic;
using UnityEngine;
using MyTools;

namespace RPG.Core
{
    [RequireComponent(typeof(SphereCollider))]
    public class EntityDetector : MonoBehaviour
    {
        [SerializeField] float scanRange = 10f;
        Dictionary<int, List<GameObject>> layeredEntities = new Dictionary<int, List<GameObject>>();
        SphereCollider spCollider;

        [SerializeField] LayerMask layerMask;

        public static EntityDetector CreateDetector(GameObject parent)
        {
            GameObject detector = new GameObject("Detector");
            detector.transform.position = parent.transform.position;
            detector.transform.parent = parent.transform;

            return detector.AddComponent<EntityDetector>();
        }

        private void Start()
        {
            spCollider = GetComponent<SphereCollider>();
            spCollider.isTrigger = true;
            spCollider.radius = scanRange;
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject gObject = other.gameObject;
            int layer = gObject.layer;

            if (!isValidLayer(layer)) return;

            if (!layeredEntities.ContainsKey(layer))
            {
                layeredEntities.Add(layer, new List<GameObject>());

            }
            if (!layeredEntities[layer].Contains(gObject))
                layeredEntities[layer].Add(gObject);
        }

        private bool isValidLayer(int layer)
        {
            int bitwiseLayer = 1 << layer;

            return (bitwiseLayer & layerMask.value) != 0;
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject gObject = other.gameObject;
            int layer = gObject.layer;

            if (!isValidLayer(layer)) return;

            if (layeredEntities.ContainsKey(layer) && layeredEntities[layer].Contains(gObject))
            {
                layeredEntities[layer].Remove(gObject);
            }
        }

        public IList<GameObject> GetEntitiesInLayer(int layer)
        {
            if (!layeredEntities.ContainsKey(layer)) return new List<GameObject>();

            for (int i = layeredEntities[layer].Count - 1; i >= 0; i--)
            {
                GameObject entity = layeredEntities[layer][i];
                if (entity == null || entity.activeSelf == false)
                {
                    layeredEntities[layer].RemoveAt(i);
                }
            }

            return layeredEntities[layer].AsReadOnly();
        }
        public GameObject GetEntityInLayer(int layer)
        {
            var entities = GetEntitiesInLayer(layer);

            if (entities.Count == 0) return null;

            return entities[0];
        }

        public GameObject GetClosestEntityInLayer(int layer)
        {
            IList<GameObject> list = GetEntitiesInLayer(layer);

            if (list == null || list.Count == 0) return null;

            return Tools.GetClosestGameObject(gameObject, list);
        }

        internal void Reset()
        {
            layeredEntities.Clear();
        }

        private float GetSquareDistance(GameObject gObject)
        {
            if (gObject == null) return -1;
            Vector3 result = transform.position - gObject.transform.position;
            return result.sqrMagnitude;
        }
    }
}