using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using RPG.Colony;

namespace RPG.UI
{
    public class UIManager : MonoBehaviour
    {
        GameObject player;
        Health playerHealth;

        GameObject nest;
        ColonyManager colony;

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                if(playerHealth!= null) playerHealth.OnDamaged+=UpdateHealthBar;
            }

            nest= GameObject.FindWithTag("Nest");
            if(nest!=null)
            {
                colony = nest.GetComponent<ColonyManager>();
                if(colony != null) colony.storage.onStoreChange += UpdateStorageText;
            }


        }

        // Update is called once per frame
        void Update()
        {

        }

        void UpdateHealthBar()
        {
            if (playerHealth == null) return;

            UIHealthBar.instance.SetValue(playerHealth.currentHealth / playerHealth.MaxHealth);
        }

        void UpdateStorageText()
        {
            if(colony == null) return;
            UIStorageText.instance.SetValue(colony.storage.storedAmount);
        }

        // called in button onClick from editor
        public void TryBuyWorker()
        {
            if(!colony.BuyWorker())
            {
                print("Not enought food");
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDamaged -= UpdateHealthBar;
            if (colony != null) colony.storage.onStoreChange += UpdateStorageText;
        }
    }
}
