using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Colony;
using RPG.Control;
using RPG.Pheromones;
using UnityEngine;
using UnityEngine.UI;

public class StartTrail : MonoBehaviour
{

    [SerializeField] Button togglePheromones;
    [SerializeField] Button startPlayerTrail;
    [SerializeField] Button[] startFollowerTrail;

    [SerializeField] ColorBlock harvestColors, combatColors;

    ColorBlock playerTrailColorH, playerTrailColorC;
    ColorBlock followerTrailColorH, followerTrailColorC;
    ColorBlock toggleColorH, toggleColorC;

    GameObject player;
    Leader leader;
    PheromoneGenerator playerGenerator;

    PheromoneType pheromoneType = PheromoneType.Harvest;

    float updateCounter;
    float updateTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerGenerator = player.GetComponent<PheromoneGenerator>();
        leader = player.GetComponent<Leader>();

        AssignListeners();

        CreateColors();

        pheromoneType = PheromoneType.Harvest;
        UpdateColors();
    }

    private void CreateColors()
    {
        playerTrailColorC = combatColors;
        playerTrailColorH = harvestColors;

        followerTrailColorC = combatColors;
        followerTrailColorH = harvestColors;

        toggleColorC = combatColors;
        toggleColorC.highlightedColor = harvestColors.highlightedColor;
        toggleColorC.pressedColor = harvestColors.pressedColor;

        toggleColorH = harvestColors;
        toggleColorH.highlightedColor = combatColors.highlightedColor;
        toggleColorH.pressedColor = combatColors.pressedColor;


    }

    private void AssignListeners()
    {
        togglePheromones.onClick.AddListener(TogglePheromones);
        startPlayerTrail.onClick.AddListener(PlayerButtonClick);

        for (int i = 0; i < startFollowerTrail.Length; i++)
        {
            Button b = startFollowerTrail[i];
            int followerIndex = i;
            b.onClick.AddListener(() => { FollowerButtonClick(followerIndex); });
        }
    }

    private void Update()
    {
        updateCounter -= Time.deltaTime;
        if (updateCounter < 0)
        {
            updateCounter = updateTime;

            if (playerGenerator.Generating)
            {
                pheromoneType = playerGenerator.PheromoneType;
            }

            int availableFollowers = leader.followerCount;
            for (int i = 0; i < startFollowerTrail.Length; i++)
            {
                Button b = startFollowerTrail[i];


                b.gameObject.SetActive(i < availableFollowers);
                b.interactable = leader.CanNotify(i);
            }

            UpdateColors();
        }

    }
    void PlayerButtonClick()
    {
        if (playerGenerator.Generating)
        {
            playerGenerator.StopGeneration();
        }
        else
        {
            playerGenerator.StartGeneration(pheromoneType);
        }
        UpdateColors();
    }

    void TogglePheromones()
    {

        if (pheromoneType == PheromoneType.Combat)
        {
            pheromoneType = PheromoneType.Harvest;
        }
        else
        {
            pheromoneType = PheromoneType.Combat;
        }

        playerGenerator.StopGeneration();

        UpdateColors();

    }

    void UpdateColors()
    {
        if(playerGenerator.Generating)
        {
            playerTrailColorC.normalColor = combatColors.highlightedColor;
            playerTrailColorC.highlightedColor = combatColors.normalColor;

            playerTrailColorH.normalColor = harvestColors.highlightedColor;
            playerTrailColorH.highlightedColor = harvestColors.normalColor;
        }
        else
        {
            playerTrailColorC.normalColor = combatColors.normalColor;
            playerTrailColorC.highlightedColor = combatColors.highlightedColor;

            playerTrailColorH.normalColor = harvestColors.normalColor;
            playerTrailColorH.highlightedColor = harvestColors.highlightedColor;
        }



        if (pheromoneType == PheromoneType.Combat)
        {
            togglePheromones.colors = toggleColorC;
            startPlayerTrail.colors = playerTrailColorC;

            foreach (Button b in startFollowerTrail)
            {
                b.colors = followerTrailColorC;
            }
        }
        else
        {
            togglePheromones.colors = toggleColorH;
            startPlayerTrail.colors = playerTrailColorH;

            foreach (Button b in startFollowerTrail)
            {
                b.colors = followerTrailColorH;
            }
        }
    }


    void FollowerButtonClick(int index)
    {
        leader.StartPheromoneTrail(index, pheromoneType);
    }
}
