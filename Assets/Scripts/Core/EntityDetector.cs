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
        List<GameObject> closeEntities = new List<GameObject>();
        Dictionary<int, List<GameObject>> layeredEntities = new Dictionary<int, List<GameObject>>();
        SphereCollider spCollider;

        [SerializeField] LayerMask layerMask;
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
            spCollider = GetComponent<SphereCollider>();
            spCollider.isTrigger = true;
            spCollider.radius = scanRange;
        }

        private void Update()
        {
            // for (int i = closeEntities.Count - 1; i >= 0; i--)    // @TODO change this method and find other way to remove null members
            // {
            //     if (closeEntities[i] == null) closeEntities.RemoveAt(i);
            // }

        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject gObject = other.gameObject;
            int layer = gObject.layer;

            if (!inLayerMask(layer)) return;

            if (!layeredEntities.ContainsKey(layer))
            {
                layeredEntities.Add(layer, new List<GameObject>());

            }
            layeredEntities[layer].Add(gObject);
        }

        private bool inLayerMask(int layer)
        {
            int bitwiseLayer = 1 << layer;

            return (bitwiseLayer & layerMask.value) != 0;
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject gObject = other.gameObject;
            int layer = gObject.layer;

            if (!inLayerMask(layer)) return;

            // if (closeEntities.Contains(other.gameObject))
            // {
            //     closeEntities.Remove(other.gameObject);
            // }

            //if (layeredEntities.ContainsKey(layer) && layeredEntities[layer].Contains(gObject))
            {
                layeredEntities[layer].Remove(gObject);
            }
        }

        public IList<GameObject> GetEntitiesInLayer(int layer)
        {
            if (!layeredEntities.ContainsKey(layer)) return new List<GameObject>();

            for (int i = layeredEntities[layer].Count - 1; i >= 0; i--)
            {
                if (layeredEntities[layer][i] == null) layeredEntities[layer].RemoveAt(i);
            }

            return layeredEntities[layer].AsReadOnly();
        }
        public GameObject GetEntityInLayer(int layer)
        {
            IList<GameObject> rList = GetEntitiesInLayer(layer);
            if (rList.Count == 0) return null;
            return rList[0];
        }

        public GameObject GetClosestEntityInLayer(int layer)
        {
            List<GameObject> list = new List<GameObject>(GetEntitiesInLayer(layer));

            Tools.SortByDistance(gameObject, ref list);

            if (list == null || list.Count == 0) return null;

            return list[0];
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