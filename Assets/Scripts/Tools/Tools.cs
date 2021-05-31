using System.Collections.Generic;
using UnityEngine;

namespace MyTools
{
    public class Tools
    {
        public static float GetSquareDistance(GameObject from, GameObject to)
        {
            if (from == null || to == null) return -1;
            Vector3 result = to.transform.position - from.transform.position;
            return result.sqrMagnitude;
        }

        public static float GetDistance(GameObject from, GameObject to)
        {
            return GetDistance(from.transform.position, to.transform.position);
        }
        public static float GetDistance(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }
        public static void SortByDistance(GameObject from, ref List<GameObject> list)
        {
            list.Sort((a, b) => GetSquareDistance(from, a).CompareTo(GetSquareDistance(from, b)));
        }

        public static GameObject GetClosestGameObject(GameObject from, ICollection<GameObject> colection)
        {
            GameObject closest = null;
            float closestDistance = float.MaxValue;
            foreach (var obj in colection)
            {
                float sqrDistance = GetSquareDistance(from, obj);

                if (sqrDistance < closestDistance)
                {
                    closest = obj;
                    closestDistance = sqrDistance;
                }
            }
            return closest;
        }
    }
}