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


        void Start()
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                if(playerHealth!= null) playerHealth.OnDamaged+=UpdateHealthBar;
            }

        }

        void UpdateHealthBar()
        {
            if (playerHealth == null) return;

            UIHealthBar.instance.SetValue(playerHealth.currentHealth / playerHealth.MaxHealth);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDamaged -= UpdateHealthBar;
        }
    }
}
