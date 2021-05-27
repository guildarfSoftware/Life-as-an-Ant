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

        public static void SortByDistance(GameObject from, ref List<GameObject> list)
        {
            list.Sort((a, b) => GetSquareDistance(from,a).CompareTo(GetSquareDistance(from,b)));
        }
    }
}