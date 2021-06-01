using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using RPG.Harvest;
using UnityEngine;
using UnityEngine.UI;

public class DropFood : MonoBehaviour
{
    [SerializeField] Button dropFoodButton;
    GameObject player;
    Harvester harvester;
    Leader leader;
    float updateCounter, updateTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        dropFoodButton.onClick.AddListener(DropFoodClick);
        player = GameObject.FindWithTag("Player");
        leader = player.GetComponent<Leader>();
        harvester = player.GetComponent<Harvester>();
    }

    private void Update()
    {
        bool showButton = !harvester.IsEmpty || leader.FollowesCarryFood();
        dropFoodButton.gameObject.SetActive(showButton);
    }

    void DropFoodClick()
    {
        harvester.DropFood();
        leader.DropFood();
    }

}
