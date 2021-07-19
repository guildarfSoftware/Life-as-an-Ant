using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDisableListener : MonoBehaviour
{
    Action onDisable;

    public void AddListener(Action action)
    {
        onDisable += action;
    }

    public void RemoveListener(Action action)
    {
        onDisable -= action;
    }

    private void OnDisable()
    {
        onDisable?.Invoke(); 
    }
}
