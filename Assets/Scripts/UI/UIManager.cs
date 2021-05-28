using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using RPG.Colony;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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


            colony = ColonyManager.instance;
            if (colony != null)
            {
                colony.onPopulationChange += UpdatePopulationText;
                colony.onPopulationChange += UpdateFoodConsumptionText;
                colony.storage.onStoreChange += UpdateStorageText;
            }

            UpdateAll();
        }
        void UpdateAll()
        {
            UpdateHealthBar();
            UpdateStorageText();
            UpdateFoodConsumptionText();
            UpdatePopulationText();
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
            string oneDecimalsText = colony.FoodRequirement.ToString("0.0");
            foodConsumption.text = $"-{oneDecimalsText}/s";
        }

        void UpdatePopulationText()
        {
            if (colony == null) return;
            string availableAnts = colony.AvailableWorkers.ToString();

            int builders = colony.BuildingAnts;
            string buildingAnts = "";
            if (builders != 0)
            {
                buildingAnts = $"({builders})";
            }
            string currentPopulation = colony.AvailableAnts.ToString();
            string maxPopulation = colony.MaxPopulation.ToString();
            workerPopulation.text = $"{currentPopulation}{buildingAnts}/{maxPopulation}";
        }
        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnHealthChange -= UpdateHealthBar;
            if (colony != null)
            {
                colony.onPopulationChange -= UpdatePopulationText;
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

        public void ExitToMenu()
        {
            MessageManager.Message("Exit to Menu", "Are you sure you want to exit and lose all progress?", _loadMenuScene, MessageManager.CloseMessage);
        }
        void _loadMenuScene()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public static void LoadMenuScene()
        {
            instance._loadMenuScene();
        }
    }
}
