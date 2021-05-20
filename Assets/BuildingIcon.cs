using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingIcon : MonoBehaviour
{
    float targetTime;
    float currentTime;
    [SerializeField] Image filling;
    Action OnTimeReached;

    private void OnDisable()
    {
        targetTime=0;
        currentTime=0;
        filling.fillAmount = 0;
        OnTimeReached = null;
    }

    private void Update()
    {
        if(targetTime == 0) return;

        currentTime += Time.deltaTime;

        filling.fillAmount = currentTime/targetTime;

        if(currentTime >= targetTime)
        {
            OnTimeReached?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void SetTime(float time)
    {
        targetTime = time;
    }

    public void AddListener(Action listener)
    {
        if(listener != null)
        {
            OnTimeReached += listener;
        }
    }
}
