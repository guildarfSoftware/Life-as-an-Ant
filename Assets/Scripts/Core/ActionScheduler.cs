using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;

        public bool StartAction(IAction action)
        {
            if (currentAction != null && currentAction != action)
            {
                if(!currentAction.isCancelable())
                {
                    return false;
                } 
                currentAction.Cancel();
            }

            currentAction = action;
            return true;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }

    }
}