using System;
using UnityEngine;

namespace MyTools
{

    public class MouseListeners : MonoBehaviour
    {
        Action MouseEnter;
        Action MouseExit;

        private void OnMouseOver()
        {
            MouseEnter?.Invoke();
        }

        private void OnMouseExit()
        {
            MouseExit?.Invoke();
        }

        public void AddOnMouseEnterListener(Action action)
        {
            MouseEnter += action;
        }
        public void RemoveOnMouseEnterListener(Action action)
        {
            MouseEnter -= action;
        }

        public void AddOnMouseExitListener(Action action)
        {
            MouseExit += action;
        }
        public void RemoveOnMouseExitListener(Action action)
        {
            MouseExit -= action;
        }

    }
}