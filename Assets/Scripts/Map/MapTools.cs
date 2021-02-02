using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            // position += new Vector3(250,0,250); //added terrain offset
            return instance.terrain.SampleHeight(position) + instance.terrain.GetPosition().y;
        }
    }
}
