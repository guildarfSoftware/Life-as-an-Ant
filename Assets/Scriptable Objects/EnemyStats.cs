using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Control;
using RPG.Core;

namespace RPG.Resources
{

    [CreateAssetMenu(fileName = "Enemy Stats")]
    public class EnemyStats : BaseStats
    {
        public Material[] materials;
        public float scale;

        public Vector3 Scale { get => new Vector3(scale, scale, scale); }
        public float FoodAmount;
    }

}