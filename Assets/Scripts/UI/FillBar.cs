using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Harvest;
using RPG.Core;
using UnityEngine.UI;
using System;
using MyTools;

public class FillBar : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] GameObject fillBar;
    [SerializeField] Image filling;
    Health health;
    HarvestTarget harvestTarget;
    MouseListeners mouseListeners;

    float updateCounter;
    float updateTime = 0.5f;

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        health = target.GetComponent<Health>();
        harvestTarget = target.GetComponent<HarvestTarget>();
        mouseListeners = target.GetComponent<MouseListeners>();

        mouseListeners.AddOnMouseEnterListener(ActivateFillBar);
        mouseListeners.AddOnMouseExitListener(DeactivateFillBar);

        DeactivateFillBar();
    }

    private void OnDisable()
    {

        mouseListeners.RemoveOnMouseEnterListener(ActivateFillBar);
        mouseListeners.RemoveOnMouseExitListener(DeactivateFillBar);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);
        
            updateCounter -= Time.deltaTime;
            if (updateCounter < 0)
            {
                UpdateFilling();
                updateCounter = updateTime;
            }
        
    }

    private void UpdateFilling()
    {
        float value;
        if (health != null && !health.IsDead)
        {
            filling.color = Color.red;
            value = health.currentHealth / health.MaxHealth;
        }
        else if (harvestTarget != null && !harvestTarget.IsEmpty)
        {
            filling.color = Color.yellow;
            value = harvestTarget.RemainingFood / harvestTarget.MaxFood;
        }
        else
        {
            fillBar.SetActive(false);
            this.enabled = false;
            value = 0;
        }

        filling.fillAmount = value;
    }

    void ActivateFillBar()
    {
        fillBar.SetActive(true);
        updateCounter = 0;
    }
    void DeactivateFillBar()
    {
        fillBar.SetActive(false);
    }
}
