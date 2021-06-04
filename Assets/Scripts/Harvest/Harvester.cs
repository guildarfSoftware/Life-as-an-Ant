using UnityEngine;
using RPG.Movement;
using RPG.Core;
using System;

namespace RPG.Harvest
{

    public class Harvester : MonoBehaviour, IAction//@TODO: separate Harvester into 3 clases harvest, storage and transport(internal capacity)
    {
        static GameObject droppedFoodPrefab;
        [SerializeField] float harvestRange = 1.0f;
        StatsManager stats;
        [SerializeField] float maxCapacity { get => ((AntStats)stats.values).CarryCapacity; }
        float carryAmount;
        public bool IsFull { get => carryAmount >= maxCapacity; }
        public bool IsEmpty { get => carryAmount == 0; }
        GameObject target;

        bool onAnimation;
        [SerializeField] GameObject food;

        internal void DropFood()
        {
            if (carryAmount == 0) return;
            food.SetActive(false);
            GameObject droppedfood = Instantiate(droppedFoodPrefab, transform.position, Quaternion.identity);
            droppedfood.GetComponent<HarvestTarget>().SetFoodAmount(carryAmount);
            carryAmount = 0;
        }

        public event Action fooodGrabbed;
        public event Action foodDeposit;

        private void Start()
        {
            if (droppedFoodPrefab == null) droppedFoodPrefab = UnityEngine.Resources.Load<GameObject>("DroppedFood");
            stats = GetComponent<StatsManager>();
        }
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

            if (IsEmpty)
            {
                Cancel();
                return;
            }

            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
            onAnimation = true;
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
            onAnimation = true;
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
            if (!GetComponent<ActionScheduler>().StartAction(this)) return;
            this.target = target;
        }

        internal bool CanStore(GameObject target)
        {
            if (target == null) return false;

            Storage storage = target.GetComponent<Storage>();
            if (storage == null) return false;
            if (carryAmount == 0) return false;
            return true;
        }

        public void Harvest(GameObject target)
        {
            if (!GetComponent<ActionScheduler>().StartAction(this)) return;
            this.target = target;
        }

        public void Cancel()
        {
            target = null;
            GetComponent<Animator>().SetTrigger("stopAttack");
            GetComponent<Animator>().ResetTrigger("attack");
            onAnimation = false;
        }

        //Animator Event
        void Hit()
        {
            if (HarvestHit())
            {
                Cancel();
                return;
            }
            if (StorageHit())
            {
                Cancel();
                return;
            }
            onAnimation = false;
        }

        private bool StorageHit()
        {
            if (target == null) return false;
            Storage storage = target.GetComponent<Storage>();
            if (storage == null) return false;
            storage.StoreResource(carryAmount);
            carryAmount = 0; //if trying to store more than capacity excess is wasted;
            food.SetActive(false);
            foodDeposit?.Invoke();
            return true;
        }

        bool HarvestHit()
        {
            if (target == null) return false;
            HarvestTarget harvestTarget = target.GetComponent<HarvestTarget>();
            if (harvestTarget == null) return false;
            carryAmount = harvestTarget.GrabResource(maxCapacity);
            food.SetActive(true);
            fooodGrabbed?.Invoke();
            return true;
        }

        public bool isCancelable()
        {
            return !onAnimation;
        }

    }

}