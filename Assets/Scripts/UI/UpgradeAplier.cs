using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Colony;
using RPG.Resources;
using System;
using System.Text;

namespace RPG.UI
{

    public class UpgradeAplier : MonoBehaviour
    {
        [SerializeField] Upgrade[] upgrades;

        [SerializeField] string titleString;
        [SerializeField] Text titleText;
        [SerializeField] Text costText;
        [SerializeField] Button button;
        [SerializeField] Text buttonText;

        bool isMaxLevel;
        bool isBuilding;

        float remainingBuildTime;

        int upgradesDone = 0;
        Upgrade currentUpgrade { get { return upgrades[upgradesDone]; } }

        float checkCostCounter = 0;
        const float checkCostTime = 0.25f;


        // Start is called before the first frame update
        void Start()
        {
            if (upgrades == null) return;
            if (titleText != null) titleText.text = titleString;
            button.onClick.AddListener(OnClick);
            buttonText.text = currentUpgrade.upgradetext;
            costText.text = GetCostText();
        }

        private void Update()
        {
            if (isMaxLevel || isBuilding) return;
            checkCostCounter -= Time.deltaTime;
            if (checkCostCounter < 0)
            {
                checkCostCounter = checkCostTime;

                button.interactable = CheckCost();
                costText.text = GetCostText();
            }
        }

        private string GetCostText()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Food Cost:  {ColonyManager.GetStoredFood().ToString("0.0")} / {currentUpgrade.foodCost}");
            if (currentUpgrade.workerCost > 0)
            {
                sb.AppendLine();
                sb.Append($"Workers needed: {ColonyManager.GetAvailableWorkersCount()}");

                if (ColonyManager.GetFollowerCount() != 0)
                {
                    sb.Append($" + {ColonyManager.GetFollowerCount()} ");
                }
                sb.Append($"/ {currentUpgrade.workerCost}");

                sb.AppendLine();

                sb.Append($"Time to upgrade: {currentUpgrade.upgradeTime} seconds");
            }
            return sb.ToString();
        }

        public void OnClick()
        {
            if(isMaxLevel||isBuilding) return;
            if (!CheckCost()) return;

            PayCost();

            if (currentUpgrade.upgradeTime > 0)
            {
                isBuilding = true;
                buttonText.text = "Building";
                button.interactable = false;
                StartCoroutine(ApplyDelayedBonus(currentUpgrade.upgradeTime));
            }
            else
            {
                ApplyBonus();
            }
        }

        bool CheckCost()
        {
            bool includeFollowers = true;
            return CheckFoodCost() && CheckAvailableAnts(includeFollowers);
        }

        bool CheckFoodCost()
        {
            return ColonyManager.GetStoredFood() >= currentUpgrade.foodCost;
        }

        bool CheckAvailableAnts(bool includeFollowers)
        {
            int availableAnts = ColonyManager.GetAvailableWorkersCount();

            if (includeFollowers)
            {
                availableAnts += ColonyManager.GetFollowerCount();
            }

            return availableAnts >= currentUpgrade.workerCost;
        }

        void PayCost()
        {
            ColonyManager.ApplyCost(currentUpgrade.foodCost, currentUpgrade.workerCost, currentUpgrade.upgradeTime);
        }

        void UpdateTier()
        {

            bool repetableClick = currentUpgrade.repeteable;

            if (!repetableClick)
            {
                if ((upgradesDone + 1) < upgrades.Length)
                {
                    upgradesDone++;
                    buttonText.text = currentUpgrade.upgradetext;
                }
                else
                {
                    isMaxLevel = true;
                    button.interactable = false;
                    buttonText.text = "MAX";
                    costText.text = "Max level reached";
                }
            }
        }

        void ApplyBonus()
        {
            switch (currentUpgrade.bonusElement)
            {
                case BonusElement.Storage:
                    {
                        ColonyManager.IncreaseMaxStorage(currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Population:
                    {
                        ColonyManager.IncreaseMaxPopulation((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Followers:
                    {
                        ColonyManager.IncreaseMaxFollowers((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Health:
                    {
                        ColonyManager.IncreaseMaxHealth(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus / 5);
                        break;
                    }
                case BonusElement.Damage:
                    {
                        ColonyManager.IncreaseMaxDamage(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus * 2);
                        break;
                    }
                case BonusElement.Speed:
                    {
                        ColonyManager.IncreaseMaxSpeed(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.CarryCapacity:
                    {
                        ColonyManager.IncreaseCarryCapacity(currentUpgrade.bonus);
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
            UpdateTier();
        }

        IEnumerator ApplyDelayedBonus(float delayedTime)
        {
            remainingBuildTime = delayedTime;
            costText.text = $"Remaining time: {remainingBuildTime}s";

            while( remainingBuildTime > 0)
            {  
                yield return new WaitForSeconds(1);
                remainingBuildTime -= 1;
                costText.text = $"Remaining time: {remainingBuildTime}s";
            }

            isBuilding = false;
            ApplyBonus();

        }

    }

}