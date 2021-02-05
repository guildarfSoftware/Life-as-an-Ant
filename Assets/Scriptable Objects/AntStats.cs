using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Control;
using RPG.Core;

namespace RPG.Core
{

    [CreateAssetMenu(fileName = "new Ant Stats")]
    public class AntStats : BaseStats
    {
        [SerializeField] float baseCarryCapacity;
        public float carryCapacityBonus;
        public float CarryCapacity => baseCarryCapacity + carryCapacityBonus;
        public float foodConsumption;

        private void Awake()
        {
            carryCapacityBonus = 0;
        }
    }

}