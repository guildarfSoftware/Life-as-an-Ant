using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using RPG.Colony;
using System;

namespace RPG.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        GameObject player;
        Health playerHealth;
        [SerializeField] GameObject upgradesMenu;
        ColonyManager colony;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                if (playerHealth != null) playerHealth.OnDamaged += UpdateHealthBar;
            }

            GameObject nest = GameObject.FindWithTag("Nest");
            if (nest != null)
            {
                colony = nest.GetComponent<ColonyManager>();
                if (colony != null) colony.storage.onStoreChange += UpdateStorageText;
            }
        }

        void UpdateHealthBar()
        {
            if (playerHealth == null) return;

            UIHealthBar.instance.SetValue(playerHealth.currentHealth / playerHealth.MaxHealth);
        }

        void UpdateStorageText()
        {
            if (colony == null) return;
            UIStorageText.instance.SetValue((int)colony.storage.storedAmount);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDamaged -= UpdateHealthBar;
            if (colony != null) colony.storage.onStoreChange += UpdateStorageText;
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
