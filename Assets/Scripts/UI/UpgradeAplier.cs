﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Colony;
using RPG.Resources;
using System;
using System.Text;
using RPG.Sounds;

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

        bool IsMaxLevel { get => upgradesDone >= upgrades.Length; }
        bool isBuilding;

        float remainingBuildTime;

        int upgradesDone = 0;
        Upgrade currentUpgrade { get { return upgrades[upgradesDone]; } }

        ColonyManager colony;

        float checkCostCounter = 0;
        const float checkCostTime = 0.25f;


        // Start is called before the first frame update
        void Start()
        {
            colony = ColonyManager.instance;
            if (upgrades == null) return;
            if (titleText != null) titleText.text = titleString;
            button.onClick.AddListener(OnClick);
            buttonText.text = currentUpgrade.upgradetext;
            costText.text = GetCostText();
        }

        private void Update()
        {
            if (upgrades[0].bonusElement == BonusElement.Followers)
            {
                upgradesDone = colony.FollowerAnts;
            }

            if (IsMaxLevel || isBuilding) return;

            checkCostCounter -= Time.deltaTime;
            if (checkCostCounter < 0)
            {
                checkCostCounter = checkCostTime;
                buttonText.text = currentUpgrade.upgradetext;


                button.interactable = CheckCost();
                costText.text = GetCostText();
            }
        }




        private string GetCostText()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Food Cost:  {colony.FoodAvailable.ToString("0.0")} / {currentUpgrade.foodCost}");
            if (currentUpgrade.workerCost > 0)
            {
                sb.AppendLine();
                sb.Append($"Workers needed: {colony.AvailableWorkers}");

                sb.Append($"/ {currentUpgrade.workerCost}");

                sb.AppendLine();

                sb.Append($"Time to upgrade: {currentUpgrade.upgradeTime} seconds");
            }
            return sb.ToString();
        }

        public void OnClick()
        {
            if (IsMaxLevel || isBuilding) return;
            if (!CheckCost()) return;



            if (currentUpgrade.bonusElement == BonusElement.Worker)
            {
                if(colony.currentPopulation >= colony.MaxPopulation)
                {
                    MessageManager.Message("Ooops", "Reached Max population. Upgrade population limit to keep growing", null, null);
                    return;
                }
                SoundEffects.PlaySound(ClipId.SpawnWorker);
            }
            else if (currentUpgrade.bonusElement == BonusElement.Followers)
            {
                if (colony.currentPopulation >= colony.MaxPopulation)
                {
                    MessageManager.Message("Ooops", "Reached Max population. Upgrade population limit to keep growing", null, null);
                    return;
                }

                SoundEffects.PlaySound(ClipId.SpawnFollower);

            }
            else
            {
                SoundEffects.PlaySound(ClipId.BuyUpgrade);
            }
            
            PayCost();

            if (currentUpgrade.upgradeTime > 0)
            {
                isBuilding = true;
                buttonText.text = "Building";
                button.interactable = false;
                if (currentUpgrade.bonusElement == BonusElement.Population)
                {
                    BuildingIconManager.StartPopulationTimer(currentUpgrade.upgradeTime);
                }
                else if (currentUpgrade.bonusElement == BonusElement.Storage)
                {
                    BuildingIconManager.StartStorageTimer(currentUpgrade.upgradeTime);
                }
                CoroutineManager.Instance.StartCoroutine(ApplyDelayedBonus(currentUpgrade.upgradeTime));
            }
            else
            {
                ApplyBonus();
            }
        }

        bool CheckCost()
        {
            bool enoughtFood = colony.FoodAvailable >= currentUpgrade.foodCost;
            bool enoughtWorkers = colony.AvailableWorkers >= currentUpgrade.workerCost;

            return enoughtFood && enoughtWorkers;
        }

        void PayCost()
        {
            colony.ApplyCost(currentUpgrade.foodCost, currentUpgrade.workerCost, currentUpgrade.upgradeTime);
        }

        void UpdateTier()
        {

            bool repetableClick = currentUpgrade.repeteable;

            if (!repetableClick)
            {
                if ((upgradesDone + 1) < upgrades.Length)
                {
                    buttonText.text = currentUpgrade.upgradetext;
                }
                else
                {
                    button.interactable = false;
                    buttonText.text = "MAX";
                    costText.text = "Max level reached";
                }
                
                if (!IsMaxLevel) upgradesDone++;
            }
        }

        void ApplyBonus()
        {
            switch (currentUpgrade.bonusElement)
            {
                case BonusElement.Storage:
                    {
                        colony.storage.IncreaseMaxCapacity(currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Population:
                    {
                        colony.IncreaseMaxPopulation((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Followers:
                    {
                        colony.AddFollowers((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Health:
                    {
                        colony.IncreaseMaxHealth(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus / 5);
                        break;
                    }
                case BonusElement.Damage:
                    {
                        colony.IncreaseMaxDamage(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus * 2);
                        break;
                    }
                case BonusElement.Speed:
                    {
                        colony.IncreaseMaxSpeed(currentUpgrade.bonus);
                        EnemyGenerator.IncreaseDifficulty((int)currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.CarryCapacity:
                    {
                        colony.IncreaseCarryCapacity(currentUpgrade.bonus);
                        break;
                    }
                case BonusElement.Worker:
                    {
                        colony.AddWorker();
                        EnemyGenerator.IncreaseDifficulty(1);
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

            while (remainingBuildTime > 0)
            {
                yield return new WaitForSeconds(1);
                remainingBuildTime -= 1;
                if (costText.isActiveAndEnabled) costText.text = $"Remaining time: {remainingBuildTime}s";
            }

            isBuilding = false;
            ApplyBonus();
            SoundEffects.PlaySound(ClipId.BuildUpgrade);

        }

    }

}