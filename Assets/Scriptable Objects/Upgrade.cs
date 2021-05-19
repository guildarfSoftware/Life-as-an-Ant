using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.UI
{
    public enum BonusElement
    {
        None,
        Storage,
        Population,
        Followers,
        Health,
        Speed,
        CarryCapacity,
        Damage,
        Worker,
        Princess,
    }

    [CreateAssetMenu(fileName = "Upgrade")]
    public class Upgrade : ScriptableObject
    {

        public string upgradetext;
        public float bonus;

        public BonusElement bonusElement;

        public float upgradeTime;
        public int foodCost, workerCost;
        public bool repeteable;

        public override string ToString()
        {
            string s = name;
            s += "( " + foodCost + " food ";
            if (workerCost != 0)
            {
                s += "/n" + workerCost + "ants";
            }
            if (upgradeTime != 0)
            {
                s += "for " + upgradeTime + "seconds";
            }

            s += ")";
            return s;
        }
    }
}