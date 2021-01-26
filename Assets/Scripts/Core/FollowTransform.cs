﻿using UnityEngine;

namespace RPG.Core
{
    public class FollowTransform : MonoBehaviour
    {
        [SerializeField]
        public Transform target;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = target.position;
        }
    }

}