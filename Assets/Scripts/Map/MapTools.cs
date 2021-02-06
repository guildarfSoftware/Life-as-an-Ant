using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Map
{

    public class MapTools : MonoBehaviour
    {
        static MapTools instance;
        Terrain terrain;

        // Start is called before the first frame update
        private void Awake()
        {
            if (instance != null) Debug.LogError("More than one Map has started");
            instance = this;
            terrain = GetComponent<Terrain>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static float getTerrainHeight(Vector3 position)
        {
            return instance.terrain.SampleHeight(position) + instance.terrain.GetPosition().y;
        }

        public static bool SampleTerrainPosition(Vector3 originPosition, out Vector3 samplesPosition)
        {
            NavMeshHit hit;
            if(NavMesh.SamplePosition(originPosition,out hit,0.5f,NavMesh.AllAreas))
            {
                samplesPosition = hit.position;
                return true;
            }
            samplesPosition = Vector3.zero;
            return false;
        }
    }
}
