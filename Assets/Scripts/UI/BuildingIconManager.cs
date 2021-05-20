using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingIconManager : MonoBehaviour
{
    [SerializeField] BuildingIcon populationIcon, storageIcon;
    static BuildingIconManager instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of BuildingIconManager");
        }
    }

    public static void StartPopulationTimer(float time)
    {
        if (instance == null) return;
        instance = FindObjectOfType<BuildingIconManager>();
        instance.StartIconTimer(time, instance.populationIcon);
    }
    public static void StartStorageTimer(float time)
    {
        if (instance == null) return;
        instance = FindObjectOfType<BuildingIconManager>();
        instance.StartIconTimer(time, instance.storageIcon);
    }

    private void StartIconTimer(float time, BuildingIcon buildingIcon)
    {
        if (buildingIcon == null) return;
        if (time < 0) return;

        buildingIcon.gameObject.SetActive(true);
        buildingIcon.transform.SetAsLastSibling();
        buildingIcon.SetTime(time);
    }
}
