using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using RPG.Colony;
using System;
using UnityEngine.UI;

namespace RPG.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        GameObject player;
        Health playerHealth;
        [SerializeField] GameObject upgradesMenu;
        ColonyManager colony;


        float timer;
        [SerializeField] Text storageText, foodConsumption, workerPopulation, princessPopulation;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                if (playerHealth != null) playerHealth.OnHealthChange += UpdateHealthBar;
            }

            GameObject nest = GameObject.FindWithTag("Nest");
            if (nest != null)
            {
                colony = nest.GetComponent<ColonyManager>();
                if (colony != null)
                {
                    colony.onPopulationChange += UpdatePopulationText;
                    colony.onPopulationChange += UpdatePrincessCountText;
                    colony.onPopulationChange += UpdateFoodConsumptionText;
                    colony.storage.onStoreChange += UpdateStorageText;
                }
            }
            UpdateAll();
        }
        void UpdateAll()
        {
            UpdateHealthBar();
            UpdateStorageText();
            UpdateFoodConsumptionText();
            UpdatePopulationText();
            UpdatePrincessCountText();
        }
        void UpdateHealthBar()
        {
            if (playerHealth == null) return;

            UIHealthBar.instance.SetValue(playerHealth.currentHealth / playerHealth.MaxHealth);
        }

        void UpdateStorageText()
        {
            if (colony == null) return;
            string currentStored = ((int)colony.storage.StoredAmount).ToString();
            string maxCapacity = colony.storage.MaxCapacity.ToString();
            storageText.text = $"{currentStored}/ {maxCapacity}";
        }
        void UpdateFoodConsumptionText()
        {
            if (colony == null) return;
            string twoDecimalsText = colony.foodRequirement.ToString("0.0");
            foodConsumption.text = $"- {twoDecimalsText}/s";
        }

        void UpdatePopulationText()
        {
            if (colony == null) return;
            string availableAnts = colony.AvailableAntsCount.ToString();
            string currentPopulation = colony.currentPopulation.ToString();
            string maxPopulation = colony.MaxPopulation.ToString();
            workerPopulation.text = $"({availableAnts}) {currentPopulation}/{maxPopulation}";
        }
        void UpdatePrincessCountText()
        {
            if (colony == null) return;
            princessPopulation.text = colony.princessPopulation.ToString();
        }
        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnHealthChange -= UpdateHealthBar;
            if (colony != null)
            {
                colony.onPopulationChange -= UpdatePopulationText;
                colony.onPopulationChange -= UpdatePrincessCountText;
                colony.onPopulationChange -= UpdateFoodConsumptionText;
                colony.storage.onStoreChange -= UpdateStorageText;
            }
        }

        public void EnableUpgradesMenu()
        {
            upgradesMenu.SetActive(true);
        }

        public void DisableUpgradesMenu()
        {
            upgradesMenu.SetActive(false);
        }
    }
}
