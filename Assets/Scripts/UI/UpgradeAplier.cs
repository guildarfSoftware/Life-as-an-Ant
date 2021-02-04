using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Colony;
using RPG.Core;

namespace RPG.UI
{

    public class UpgradeAplier : MonoBehaviour
    {
        [SerializeField] Upgrade upgrade;
        // Start is called before the first frame update
        void Start()
        {
            if (upgrade == null) return;
            Text text = GetComponentInChildren<Text>();
            if (text != null) text.text = upgrade.ToString();
            Button button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }


        public void OnClick()
        {
            if (!CheckCost()) return;
            
            bool repetableClick = upgrade.bonusElement != BonusElement.Worker && upgrade.bonusElement != BonusElement.Princess;

            if(!repetableClick) GetComponent<Button>().interactable = false;
            
            
            if (upgrade.upgradeTime > 0)
            {
                Invoke("ApplyBonus", upgrade.upgradeTime);
            }
            else
            {
                ApplyBonus();
            }
            PayCost();
        }

        bool CheckCost()
        {
            return ColonyManager.CheckCost(upgrade.foodCost, upgrade.workerCost);
        }

        void PayCost()
        {
            ColonyManager.ApplyCost(upgrade.foodCost, upgrade.workerCost, upgrade.upgradeTime);
        }

        void ApplyBonus()
        {
            switch (upgrade.bonusElement)
            {
                case BonusElement.Storage:
                    {
                        ColonyManager.IncreaseMaxStorage(upgrade.bonus);
                        break;
                    }
                case BonusElement.Population:
                    {
                        ColonyManager.IncreaseMaxPopulation((int)upgrade.bonus);
                        break;
                    }
                case BonusElement.Followers:
                    {
                        ColonyManager.IncreaseMaxFollowers((int)upgrade.bonus);
                        break;
                    }
                case BonusElement.Health:
                    {
                        AntStats.IncreaseMaxHealth(upgrade.bonus);

                        break;
                    }
                case BonusElement.Damage:
                    {

                        AntStats.IncreaseMaxDamage(upgrade.bonus);
                        break;
                    }
                case BonusElement.Speed:
                    {

                        AntStats.IncreaseMaxSpeed(upgrade.bonus);
                        break;
                    }
                case BonusElement.CarryCapacity:
                    {

                        AntStats.IncreaseCarryCapacity(upgrade.bonus);
                        break;
                    }
                case BonusElement.Worker:
                    {
                        ColonyManager.AddWorker();
                        break;
                    }
                case BonusElement.Princess:
                    {

                        ColonyManager.AddPrincess();
                        break;
                    }
                case BonusElement.None:
                default:
                    {
                        return;
                    }
            }
        }
    }

}