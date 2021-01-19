using UnityEngine;
using RPG.Core;
using RPG.Pheromones;
using RPG.Harvest;
using RPG.Combat;
using System;
using System.Collections.Generic;
using RPG.Movement;

namespace RPG.Control
{
    public class WorkerController : MonoBehaviour
    {
        EntityDetector detector;
        PheromoneFollower follower;
        Harvester harvester;
        Fighter fighter;

        Explorer explorer;
        GameObject nest;

        private void Start()
        {
            detector = GetComponent<EntityDetector>();
            follower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            explorer = GetComponent<Explorer>();
            nest = GameObject.FindGameObjectWithTag("Nest");
        }
        private void Update()
        {

            detector.Sort((a, b) => GetDistance(a).CompareTo(GetDistance(b)));  //sort close entities by distance

            GameObject target = null;
            if (!harvester.IsEmpty && harvester.CanStore(nest))
            {
                harvester.Store(nest);
                return;
            }

            target = GetClosestEntityWithTag("Enemy");
            if (target != null && fighter.CanAttack(target))
            {
                GetComponent<Fighter>().Attack(target);
                follower.Cancel();   // to avoid mark as invalid last waypoint
                return;
            }

            target = GetClosestEntityWithTag("Food");
            if (target != null && harvester.CanHarvest(target))
            {
                GetComponent<Harvester>().Harvest(target);
                follower.Cancel();   // to avoid mark as invalid last waypoint
                return;
            }

            if (!follower.routeEnded)
            {
                return; //route not finished kepp following pheromones
            }
            
            if (follower.lastWaypoint != null) follower.lastWaypoint.MarkAsInvalid();  //route has ended and cannot find food or enemy

            target = GetClosestEntityWithTag("PheromoneCombat");
            if (target != null && follower.CanFollow(target))
            {
                follower.StartRoute(target.GetComponent<PheromoneWaypoint>());
                return;
            }

            target = GetClosestEntityWithTag("PheromoneHarvest");
            if (target != null && follower.CanFollow(target))
            {
                follower.StartRoute(target.GetComponent<PheromoneWaypoint>());
                return;
            }

            if(!explorer.onCooldown) explorer.Wander(10); //explore around position for 10 seconds

            if(!explorer.TimeOut)
            {
                return;
            }
            
            GetComponent<Mover>().MoveTo(nest.transform.position);

        }


        private GameObject GetClosestEntityWithTag(string tag)
        {
            List<GameObject> list = detector.GetEntitiesWithTag(tag);
            
            if(list==null || list.Count==0) return null;
            
            return list[0];
        }

        private float GetDistance(GameObject gObject)
        {
            if (gObject == null) return -1;
            return Vector3.Distance(transform.position, gObject.transform.position);
        }
    }
}