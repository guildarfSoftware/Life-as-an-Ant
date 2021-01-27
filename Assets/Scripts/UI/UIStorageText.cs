using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class UIStorageText : MonoBehaviour
    {
        public static UIStorageText instance { get; private set; }
        [SerializeField] Text storageAmount;
        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        public void SetValue(int value)
        {
            storageAmount.text = value.ToString();
        }

    }

}
