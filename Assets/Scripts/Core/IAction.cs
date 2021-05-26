using UnityEngine;

namespace RPG.Core
{
    public interface IAction 
    {
        bool isCancelable();
        void Cancel(); 
    }
}