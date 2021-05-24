using UnityEngine;

namespace RPG.Core
{
    public class FollowTransform : MonoBehaviour
    {
        [SerializeField] Transform target;

        void LateUpdate()
        {
            transform.position = target.position;
        }
    }

}