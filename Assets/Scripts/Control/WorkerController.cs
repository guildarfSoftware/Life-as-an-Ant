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
        GameObject nest;
        List<GameObject> entitiesInRange;

        private void Start()
        {
            detector = GetComponent<EntityDetector>();
            follower = GetComponent<PheromoneFollower>();
            harvester = GetComponent<Harvester>();
            fighter = GetComponent<Fighter>();
            nest = GameObject.FindGameObjectWithTag("Nest");
        }
        private void Update()
        {
            entitiesInRange = GetEntitiesInRangeSorted();

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

            GetComponent<Mover>().MoveTo(nest.transform.position);

            /*
                arriving here with an active pheromone indicates food or enemy is gone, destroy trail
            */

            /*
                Explore
            */

        }


        private GameObject GetClosestEntityWithTag(string tag)
        {
            for (int i = 0; i < entitiesInRange.Count; i++)
            {
                GameObject entity = entitiesInRange[i];
                if (entity != null && entity.tag == tag) return entity;
            }

            return null;
        }


        private List<GameObject> GetEntitiesInRangeSorted()
        {
            var entityList = detector.CloseEntities;

            if (entityList == null) return null;

            entityList.Sort((a, b) => GetDistance(a).CompareTo(GetDistance(b)));

            return entityList;

        }

        private float GetDistance(GameObject gObject)
        {
            if (gObject == null) return -1;
            return Vector3.Distance(transform.position, gObject.transform.position);
        }
    }
}