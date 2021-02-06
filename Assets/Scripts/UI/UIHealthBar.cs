using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class UIHealthBar : MonoBehaviour
    {
        public static UIHealthBar instance { get; private set; }
        [SerializeField] Image mask;
        float originalSize;

        private void Awake()
        {
            instance = this;
            originalSize = mask.rectTransform.rect.height;
        }

        public void SetValue(float value)
        {
            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
        }
    }
}
