using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{

    public class StatsManager : MonoBehaviour
    {
        public BaseStats values;

        public void Initialize()
        {
            values.ResetBonus();
        }

    }
}
