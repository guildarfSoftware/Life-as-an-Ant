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

    GameObject player;
    Leader leader;
    PheromoneGenerator playerGenerator;

    PheromoneType type = PheromoneType.Harvest;

    float updateCounter;
    float updateTimer = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerGenerator = player.GetComponent<PheromoneGenerator>();
        leader = player.GetComponent<Leader>();

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
            int availableFollowers = leader.followerCount;
            for (int i = 0; i < startFollowerTrail.Length; i++)
            {
                Button b = startFollowerTrail[i];


                b.gameObject.SetActive(i < availableFollowers);
                b.interactable = leader.CanNotify(i);


            }
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
            playerGenerator.StartGeneration(type);
        }
    }

    void TogglePheromones()
    {
        ColorBlock colors;

        if (type == PheromoneType.Combat)
        {
            type = PheromoneType.Harvest;
            colors = harvestColors;
        }
        else
        {
            type = PheromoneType.Combat;
            colors = combatColors;
        }

        togglePheromones.colors = colors;
        startPlayerTrail.colors = colors;

        foreach (Button b in startFollowerTrail)
        {
            b.colors = colors;
        }

    }

    void FollowerButtonClick(int index)
    {
        leader.StartPheromoneTrail(index, type);
    }
}
