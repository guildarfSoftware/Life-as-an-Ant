using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDestroyListener : MonoBehaviour
{
    Action onDestroy;

    public void AddListener(Action action)
    {
        onDestroy += action;
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
