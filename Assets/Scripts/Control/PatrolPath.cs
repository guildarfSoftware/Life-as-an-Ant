using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Map;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float wayPointGizmosRadius = 0.3f;
        int currentWaypoint;

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform waypoint = transform.GetChild(i);
                Vector3 position = waypoint.position;
                position.y = MapTools.getTerrainHeight(position);
                waypoint.position=position;
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < transform.childCount; i++)
            {
                int nextChild = GetNextWaypointIndex(i);
                Gizmos.DrawSphere(GetWaypointPosition(i), wayPointGizmosRadius);
                Gizmos.DrawLine(GetWaypointPosition(i), GetWaypointPosition(nextChild));
            }
        }

        public int GetNextWaypointIndex(int i)
        {
            int j = i + 1;
            if (j == transform.childCount)
            {
                j = 0;
            }
            return j;
        }

        public Vector3 GetWaypointPosition(int i)
        {
            return transform.GetChild(i).transform.position;
        }
    }
}
