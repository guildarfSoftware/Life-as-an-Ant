using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Colony;
using RPG.Resources;

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
            
            bool repetableClick = upgrade.bonusElement == BonusElement.Worker || upgrade.bonusElement == BonusElement.Princess;

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
                        ColonyManager.IncreaseMaxHealth(upgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)upgrade.bonus /5);
                        break;
                    }
                case BonusElement.Damage:
                    {
                        ColonyManager.IncreaseMaxDamage(upgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)upgrade.bonus * 2);
                        break;
                    }
                case BonusElement.Speed:
                    {
                        ColonyManager.IncreaseMaxSpeed(upgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)upgrade.bonus);
                        break;
                    }
                case BonusElement.CarryCapacity:
                    {
                        ColonyManager.IncreaseCarryCapacity(upgrade.bonus);
                        break;
                    }
                case BonusElement.Worker:
                    {
                        ColonyManager.AddWorker();
                        EnemyGenerator.IncreaseDifficulty(1);
                        break;
                    }
                case BonusElement.Princess:
                    {
                        ColonyManager.AddPrincess();
                        EnemyGenerator.IncreaseDifficulty(3);
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