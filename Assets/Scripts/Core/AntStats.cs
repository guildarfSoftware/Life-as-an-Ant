using System;
using UnityEngine;

namespace RPG.Core
{
    public static class AntStats
    {
        public static float Damage=5, Speed=8, CarryCapacity=5, Health=20;

        internal static void IncreaseMaxHealth(float bonus)
        {
            Health += bonus;
        }

        internal static void IncreaseMaxDamage(float bonus)
        {
            Damage += bonus;
        }

        internal static void IncreaseMaxSpeed(float bonus)
        {
            Speed += bonus;
        }

        internal static void IncreaseCarryCapacity(float bonus)
        {
            CarryCapacity += bonus;
        }
    }
}