using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Harvest;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class cursor : MonoBehaviour
{
    [SerializeField] Sprite attackSp, gatherSp, storeSp, moveSp,normalSp;
    [SerializeField] GameObject player;
    Fighter playerFighter;
    Harvester playerHarvester;
    public Vector3 offset;
    Image spRenderer;
    public Canvas myCanvas;
    void Start()
    {
        //Cursor.visible = false;
        spRenderer = GetComponent<Image>();
        playerFighter = player.GetComponent<Fighter>();
        playerHarvester = player.GetComponent<Harvester>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
        transform.position = myCanvas.transform.TransformPoint(pos);


        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(mouseRay);

        bool attack = false, store = false, harvest = false, move = false;

        foreach (RaycastHit hit in hits)
        {

            CombatTarget targetC = hit.transform.GetComponent<CombatTarget>();  // todo refactored this due to changes in figther need to use combat target again 
            if (targetC != null && playerFighter.CanAttack(targetC.gameObject))
            {
                attack = true;
            }


            Storage targetS = hit.transform.GetComponent<Storage>();
            if (targetS != null && playerHarvester.CanStore(targetS.gameObject))
            {
                store = true;
            }


            HarvestTarget targetH = hit.transform.GetComponent<HarvestTarget>();
            if (targetH != null && playerHarvester.CanHarvest(targetH.gameObject))
            {
                harvest = true;
            }


            GameObject targetM = hit.transform.gameObject;
            if (targetM != null && targetM.name == "Terrain")
            {
                move = true;
            }
        }

        if (attack) spRenderer.sprite = attackSp;
        else if (store) spRenderer.sprite = storeSp;
        else if (harvest) spRenderer.sprite = gatherSp;
      //  else if (move) spRenderer.sprite = moveSp;
        else spRenderer.sprite = normalSp;

    }
}
