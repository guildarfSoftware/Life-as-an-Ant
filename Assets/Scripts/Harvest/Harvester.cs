using UnityEngine;
using RPG.Movement;
using RPG.Core;
using System;

namespace RPG.Harvest
{
    public class Harvester : MonoBehaviour, IAction//@TODO: separate Harvester into 3 clases harvest, storage and transport(internal capacity)
    {
        [SerializeField] float harvestRange = 2.0f;
        [SerializeField] float maxCapacity = 7;
        float carryAmount;
        public bool IsFull { get => carryAmount >= maxCapacity; }
        public bool IsEmpty { get => carryAmount == 0; }
        GameObject target;

        private void Update()
        {
            if (target == null) return;
            if (GetIsInRange())
            {
                GetComponent<Mover>().Cancel();
                Behaviour();
            }
            else
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }

        }

        void Behaviour()
        {
            if (CanHarvest(target))
            {
                HarvestBehaviour();
                return;
            }

            if (CanStore(target))
            {
                StorageBehaviour();
                return;
            }

        }

        private void StorageBehaviour()
        {
            Storage storage = target.GetComponent<Storage>();
            LookAt(storage.transform);

            if (storage.IsFull || IsEmpty)
            {
                Cancel();
                return;
            }

            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
            Hit();// @ToDo: integrate Hit event in animation
        }

        private void HarvestBehaviour()
        {
            HarvestTarget harvestTarget = target.GetComponent<HarvestTarget>();
            LookAt(harvestTarget.transform);

            if (harvestTarget.IsEmpty || IsFull)
            {
                Cancel();
                return;
            }

            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
            Hit();// @ToDo: integrate Hit event in animation
        }

        private void LookAt(Transform target)//Looks at transform only rotating in Y axys
        {
            Quaternion rotation = transform.rotation;
            transform.LookAt(target);

            rotation.y = transform.rotation.y;
            transform.rotation = rotation;
        }

        private bool GetIsInRange()
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.transform.position) < harvestRange;
        }

        public bool CanHarvest(GameObject target)
        {
            if (target == null) return false;

            Health health = target.GetComponent<Health>();
            if (health != null && !health.IsDead) return false;   //only dead entities can be harvest

            HarvestTarget harvestTarget = target.GetComponent<HarvestTarget>();
            if (harvestTarget == null) return false;
            if (harvestTarget.IsEmpty) return false;
            return carryAmount < maxCapacity;
        }

        internal void Store(GameObject target)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            this.target = target;
            print("Storing");
        }

        internal bool CanStore(GameObject target)
        {
            if (target == null) return false;

            Storage storage = target.GetComponent<Storage>();
            if (storage == null) return false;
            if (storage.IsFull) return false;
            return carryAmount != 0;
        }

        public void Harvest(GameObject target)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            this.target = target;
            print("Harvesting");
        }

        public void Cancel()
        {
            target = null;
            GetComponent<Animator>().SetTrigger("stopAttack");
            GetComponent<Animator>().ResetTrigger("attack");
        }

        public void StartAction()
        {
            throw new System.NotImplementedException();
        }

        //Animator Event
        void Hit()
        {
            if (HarvestHit()) return;
            if (StorageHit()) return;
        }

        private bool StorageHit()
        {
            if (target == null) return false;
            Storage storage = target.GetComponent<Storage>();
            if (storage == null) return false;
            storage.StoreResource(carryAmount);
            carryAmount = 0; //if trying to store more than capacity excess is wasted;
            print("carrying: " + carryAmount + "/" + maxCapacity);
            return true;
        }

        bool HarvestHit()
        {
            if (target == null) return false;
            HarvestTarget harvestTarget = target.GetComponent<HarvestTarget>();
            if (harvestTarget == null) return false;
            carryAmount = harvestTarget.GrabResource(maxCapacity);
            print("carrying: " + carryAmount + "/" + maxCapacity);
            return true;
        }

    }

}