using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Harvest;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class cursor : MonoBehaviour
{
    [SerializeField] Sprite attackSp, gatherSp, storeSp, moveSp, normalSp;
    [SerializeField] GameObject player;
    Fighter playerFighter;
    Harvester playerHarvester;
    public Vector3 offset;
    Image image;
    public Canvas myCanvas;
    void Start()
    {
        Cursor.visible = false;
        image = GetComponent<Image>();
        playerFighter = player.GetComponent<Fighter>();
        playerHarvester = player.GetComponent<Harvester>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
        RectTransform rectTransform = (transform as RectTransform);
        pos.x += rectTransform.rect.width / 2;
        pos.y -= rectTransform.rect.height / 2;
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

        if (EventSystem.current.IsPointerOverGameObject()) image.sprite = normalSp;
        else if (attack) image.sprite = attackSp;
        else if (harvest) image.sprite = gatherSp;
        else if (store) image.sprite = storeSp;
        else if (move) image.sprite = moveSp;
        else image.sprite = normalSp;

    }

    private void OnDisable()
    {
        Cursor.visible = true;        
    }
}
